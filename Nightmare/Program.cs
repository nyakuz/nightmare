using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Quic;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.AspNetCore.StaticFiles;
using Nightmare.Main;


AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs args) => {
  var exception = (Exception)args.ExceptionObject;
  Console.WriteLine($"Unhandled exception occurred: {exception.Message}");
};

var builder = WebApplication.CreateBuilder(new WebApplicationOptions() {
  Args = args,
  ContentRootPath = Directory.GetCurrentDirectory(),
  WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
});
Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), "vhost", "null"));

builder.WebHost.ConfigureKestrel((context, options) => {
  options.AllowSynchronousIO = true;
  options.AddServerHeader = context.Configuration.GetValue("Kestrel:AddServerHeader", false);
  options.Limits.MaxRequestBodySize = context.Configuration.GetValue("Kestrel:Limits:MaxRequestBodySize", 104857600); // only multipart/form-data 104857600 == 100mb
});

builder.Services.AddHealthChecks();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
/*builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression(options => {
  options.EnableForHttps = true;
  options.Providers.Add<BrotliCompressionProvider>();
  options.Providers.Add<GzipCompressionProvider>();
  options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
    "application/octet-stream"
  });
});*/
builder.Services.Configure<SocketTransportOptions>(options => {
  options.CreateBoundListenSocket = endpoint => {
    var socket = SocketTransportOptions.CreateDefaultBoundListenSocket(endpoint);
    return socket;
  };
});
builder.Services.Configure<QuicTransportOptions>(options => {
  
});
/*builder.Services.Configure<GzipCompressionProviderOptions>(options => {
  options.Level = System.IO.Compression.CompressionLevel.Fastest;
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => {
  options.Level = System.IO.Compression.CompressionLevel.Fastest;
});*/
builder.Services.Configure<StaticFileOptions>(options => {
  options.ServeUnknownFileTypes = true;
  options.ContentTypeProvider = new FileExtensionContentTypeProvider();
  options.DefaultContentType = "text/plain";
});

builder.Services.AddSession(options => {
  options.Cookie.Name = SecureService.Sess;
  options.IdleTimeout = TimeSpan.FromSeconds(10);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});
builder.Services.AddDataProtection().UseCryptographicAlgorithms(new());
builder.Services.AddAntiforgery(options => {
  options.Cookie.Name = SecureService.CSRF;
  options.HeaderName = SecureService.XSRF;
});
builder.Services.AddControllers(options => {
  options.InputFormatters.Add(new TextPlain());
});
builder.Services.AddSingleton<MimeTypeSingleton>();
builder.Services.AddSingleton(new MainRouter(builder.Configuration, builder.Services, builder.Environment.ContentRootPath));
builder.Services.AddScoped<ISecureService, SecureService>();

builder.Services.AddControllersWithViews();
/*#if DEBUG
  builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
#else
  builder.Services.AddRazorPages();
#endif*/


var app = builder.Build();
var webServerConfig = app.Configuration.GetSection("AppSettings:WebServer");
var signalRConfig = app.Configuration.GetSection("AppSettings:SignalR");
var errorpath = webServerConfig.GetValue("ErrorPageFilePath", "/error")!;
var healthpath = webServerConfig.GetValue("HealthPageFilePath", "/healthz")!;

// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
#if DEBUG
  app.UseDeveloperExceptionPage();
#else
  app.UseExceptionHandler(errorpath);
  app.UseHsts();
#endif

/*app.Use((context, next) => {
  var feature = context.Features.GetRequiredFeature<IHttpWebTransportFeature>();

  context.Response.Headers.AltSvc = $"h3=\":{context.Request.Host.Port}\"";

  if(!feature.IsWebTransportRequest) {
    return next(context);
  }

  var session = feature.AcceptAsync(CancellationToken.None).Result;
  var stream = session.OpenUnidirectionalStreamAsync(CancellationToken.None).Result;

  return next(context);
});*/
app.UseSession();
//app.UseResponseCaching();
//app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<MainMiddleware>();

app.MapControllers();
app.MapHealthChecks(healthpath).DisableHttpMetrics();
//app.MapRazorPages();
app.MapHub<MainHub>("/~", options => {
  var transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.None;
  foreach(var typename in signalRConfig.GetSection("TransportType").Get<string[]>() ?? ["ServerSentEvents", "LongPolling"]) {
    if(Enum.TryParse(typename, out Microsoft.AspNetCore.Http.Connections.HttpTransportType value) == true) {
      transports |= value;
    }
  }
  options.Transports = transports;
});

app.Run();