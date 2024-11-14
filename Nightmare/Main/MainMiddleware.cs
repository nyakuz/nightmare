using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using XSAPI;

namespace Nightmare.Main {
	public class MainMiddleware {
		private readonly RequestDelegate _next;

		public class PageSapiContext : PageSapi, IPage {
			public readonly HttpContext Ctx;
			public readonly MimeTypeSingleton MimeType;
			public readonly ISecureService Session;
			public string VhostName = string.Empty;
			public string RequestMethod = string.Empty;
			public string RequestQueryString = string.Empty;

			public override string VirtualHostName { get => VhostName; }
			public override string RequestHost { get => Ctx.Request.Host.Host; }
			public override string RequestPath { get => Ctx.Request.Path.Value ?? string.Empty; }
			public override bool HasStarted { get => Ctx.Response.HasStarted; }

			public PageSapiContext(HttpContext context, MimeTypeSingleton mime_type, ISecureService session) {
				Ctx = context;
				MimeType = mime_type;
				Session = session;

				var req = context.Request;
				var query = req.QueryString.ToString();
				RequestQueryString = query.Length > 0 ? query[1..] : string.Empty;
				RequestMethod = req.Method;
			}

			public override dynamic? GetProperty(Type method_type) {
				var type = Ctx.GetType();
				if (method_type.IsAssignableFrom(type) == true) {
					return Ctx;
				}

				var properties = type.GetProperties();
				for (var i = 0; i < properties.Length; i++) {
					var prop = properties[i];

					if (method_type.IsAssignableFrom(prop.PropertyType) == true) {
						return prop.GetValue(Ctx);
					}
				}

				return null;
			}
			public override dynamic? GetService(Type service_type) {
				return Ctx.RequestServices.GetService(service_type);
			}
			public override string? GetQuery(string name) {
				if (Ctx.Request.Query.TryGetValue(name, out var value) == true) {
					return value;
				}

				return null;
			}

			public Dictionary<string, dynamic> GetAllSecureCookie() {
				var secure = (ISecureService)Ctx.RequestServices.GetService(typeof(ISecureService))!;

				return secure.Load(Ctx.Request.Cookies);
			}
			public dynamic GetSecureCookie(string property_name) {
				var secure = (ISecureService)Ctx.RequestServices.GetService(typeof(ISecureService))!;

				secure.Get(Ctx.Request.Cookies, property_name, out var value);
				return value;
			}
			public void SetSecureCookie(Dictionary<string, dynamic> data) {
				var secure = (ISecureService)Ctx.RequestServices.GetService(typeof(ISecureService))!;

				secure.Save(Ctx.Response.Cookies, data);
			}
			public void DeleteSecureCookie() {
				var secure = (ISecureService)Ctx.RequestServices.GetService(typeof(ISecureService))!;

				secure.Delete(Ctx.Response.Cookies);
			}

			public override void GetParameter(out string method, out string content_type, out long content_length, out string query_string) {
				var req = Ctx.Request;

				method = RequestMethod;
				content_type = req.ContentType ?? string.Empty;
				content_length = req.ContentLength ?? 0;
				query_string = RequestQueryString;
			}

			public override void SetHttpState(int code) {
				Ctx.Response.StatusCode = code;
			}

			public override async ValueTask<string> ReadBodyTextPlain() {
				var size = Ctx.Request.ContentLength ?? 0;

				if (size == 0) return string.Empty;

				var builder = new StringBuilder((int)size);
				var reader = Ctx.Request.BodyReader;
				var decoder = Encoding.UTF8.GetDecoder();
				var buffer = new ArrayBufferWriter<char>();
				ReadResult result;

				do {
					result = await reader.ReadAsync();
					decoder.Convert(result.Buffer, buffer, false, out var _, out var _);
					builder.Append(buffer.WrittenSpan.ToString());
					buffer.Clear();
					reader.AdvanceTo(result.Buffer.End);
				} while (result.IsCompleted == false);

				await reader.CompleteAsync();

				return builder.ToString();
			}

			public override Task ResponseFile(string filepath, string fileext) {
				if (MimeType.TryGetName(fileext, out var mime)) {
					Ctx.Response.Headers.ContentType = mime;
				}

				return Ctx.Response.SendFileAsync(filepath, Ctx.RequestAborted);
			}


			public override void SendHeaderCallback(string name, string value) {
				Ctx.Response.Headers.Append(name, value);
			}
			public override long UbWriteCallback(ReadOnlySpan<byte> str, long str_length) {
				Ctx.Response.BodyWriter.Write(str);
				return str_length;
			}
			public async override ValueTask<int> SendFlushCallback() {
				await Ctx.Response.BodyWriter.FlushAsync(Ctx.RequestAborted);
				return 0;
			}
			public override ValueTask<long> ReadPostCallback(nint buf, long count_bytes) {
				var reader = Ctx.Request.BodyReader;
				var result = reader.ReadAsync().Result;
				var buffer = result.Buffer;
				var length = Math.Min(buffer.Length, count_bytes);
				var span = ToSpan(buf, (int)count_bytes);

				buffer.Slice(0, length).CopyTo(span);
				reader.AdvanceTo(buffer.Start, buffer.GetPosition(length));

				if (result.IsCompleted == true) {
					reader.Complete();
				}

				return ValueTask.FromResult(length);
			}
			public override string ReadCookiesCallback() {
				var request = Ctx.Request;
				var cookie_value = SecureService.CookieName + "=" + Session.LoadToJsonString(request.Cookies);

				if (request.Headers.TryGetValue("Cookie", out var cookie2) == true) {
					cookie_value += "; ";
					cookie_value += cookie2.ToString();
				}

				return cookie_value;
			}
			public override void ServerParamCallback(Action<string, string> feedback) {
				var request = Ctx.Request;
				var conn = Ctx.Connection;
				string? ct = null;
				string cl = string.Empty;

				foreach (var (key, val) in request.Headers) {
					if (key == null) continue;
					feedback("HTTP_" + key.ToUpper().Replace('-', '_'), val.ToString());

					switch (key.ToUpper()) {
					case "CONTENT-TYPE": ct = val.ToString(); break;
					case "CONTENT-LENGTH": cl = val.ToString(); break;
					}
				}

				if (ct != null) {
					feedback("CONTENT_TYPE", ct);
				}
				if (cl != string.Empty) {
					feedback("CONTENT_LENGTH", cl);
				}

				if (request.IsHttps == true) {
					feedback("HTTPS", "on");
				}

				feedback("SERVER_ADDR", $"{conn.LocalIpAddress}");
				feedback("SERVER_PORT", $"{conn.LocalPort}");
				feedback("REMOTE_ADDR", $"{conn.RemoteIpAddress}");
				feedback("REMOTE_PORT", $"{conn.RemotePort}");
				feedback("PATH", VhostDirectory);
				feedback("SERVER_PROTOCOL", request.Protocol);
				feedback("REQUEST_METHOD", RequestMethod);
				feedback("QUERY_STRING", RequestQueryString);
				feedback("REQUEST_URI", request.GetEncodedPathAndQuery());
				feedback("SCRIPT_NAME", VhostFilePath);
				feedback("SCRIPT_FILENAME", VhostFullPath);
				feedback("DOCUMENT_ROOT", VhostDirectory);
			}
		}

		static MainMiddleware() {
			Console.WriteLine("Vhost: {0}", MainRouter.DefaultVhostDirectory);
		}

		public MainMiddleware(RequestDelegate next) {
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, MainRouter router, MimeTypeSingleton mimeType, ISecureService session) {
			var res = context.Response;
			
			res.Headers.AltSvc = $"h3=\":{context.Request.Host.Port}\"";
			await _next(context); // TODO: Request reached the end of the middleware pipeline without being handled by application code.

			if (res.Headers.ContentType.Count == 0 && res.StatusCode == (int)HttpStatusCode.NotFound && res.HasStarted == false) {
				var sapi = new PageSapiContext(context, mimeType, session);

				await router.InvokePage(sapi);
				await res.BodyWriter.FlushAsync();
			}
		}
	}
}
