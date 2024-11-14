using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using XSAPI;

namespace XPhpModule {
  public class XPhpScript: Module {
    public const int PHP_SUCCESS = 0;
    public readonly static ConcurrentDictionary<nuint, Sapi> CaseScript = new();


    static XPhpScript() {
      var code = On(
        EventSendHeader,
        EventUbWrite,
        EventFlush,
        EventReadPost,
        EventReadCookies,
        EventServerParam);

      if(code != PHP_SUCCESS) {
        Console.Error.WriteLine("XPhpScript: Error Code {0}", code);
      }
    }

    static Distructor distructor = new();
    class Distructor {
      ~Distructor() {
        Off();
      }
    }


    public override IEnumerable<string> ScriptFileExtension { get => [".php", ".inc"]; }
    public override IEnumerable<string> FileWatchExtension { get { yield break; } }
    public override ValueTask<int> InvokePage(PageSapi sapi) {
      var context = tsrm_thread_id();
      
      sapi.GetParameter(out var method, out var content_type, out var content_length, out var query_string);
      sapi.GetVhostPath(out var script_filename_index, out var script_filepath);

      CaseScript[context] = sapi;
      var code = Execute(context, method, content_type, content_length, script_filename_index, script_filepath, query_string);

      CaseScript.TryRemove(context, out var _);

      return ValueTask.FromResult(code);
    }

    public override ValueTask<int> InvokeHubConnected(IHub hub) {

      return ValueTask.FromResult(0);
    }

    public override ValueTask<int> InvokeHubDisconnected(IHub hub) {

      return ValueTask.FromResult(0);
    }

    public override ValueTask<int> InvokeHubDataReceived(IHub hub, string req_path, params dynamic[] args) {

      return ValueTask.FromResult(0);
    }

    public override int InvokeEvent(EventName event_name, EventMethod event_method, string hostname, params dynamic[] arguments) {
      return 0;
    }


    public static SendHeader EventSendHeader = SendHeaderCallback;
    unsafe public static UbWrite EventUbWrite = UbWriteCallback;
    public static Flush EventFlush = FlushCallback;
    unsafe public static ReadPost EventReadPost = ReadPostCallback;
    public static ReadCookies EventReadCookies = ReadCookiesCallback;
    public static ServerParam EventServerParam = ServerParamCallback;

    public static void SendHeaderCallback(nuint context, string head, long str_length) {
      var tmp = head.Split(": ");

      if(tmp.Length < 2) return;

      switch(tmp[0]) {
      case "Status":
        if(tmp[1].Length >= 3 && int.TryParse(tmp[1].Substring(0, 3), out var code) == true) {
          ((PageSapi)CaseScript[context]).SetHttpState(code);
        }
        break;
      default:
        ((PageSapi)CaseScript[context]).SendHeaderCallback(tmp[0], tmp[1]);
        break;
      }
    }
    unsafe public static long UbWriteCallback(nuint context, byte* str, long str_length) {
      try {
        var chunk = new ReadOnlySpan<byte>(str, (int)str_length);
        return ((PageSapi)CaseScript[context]).UbWriteCallback(chunk, str_length);
      } catch { }
      return str_length;
    }
    public static int FlushCallback(nuint context) {
      try {
        return ((PageSapi)CaseScript[context]).SendFlushCallback().Result;
      } catch { }
      return 0;
    }
    unsafe public static long ReadPostCallback(nuint context, byte* buf, long count_bytes) {
      try {
        var buf2 = new Span<byte>(buf, (int)count_bytes);
        return ((PageSapi)CaseScript[context]).ReadPostCallback((nint)buf, count_bytes).Result;
      } catch { }
      return 0;
    }
    public static string ReadCookiesCallback(nuint context) {
      try {
        return ((PageSapi)CaseScript[context]).ReadCookiesCallback();
      } catch { }
      return string.Empty;
    }
    public static void ServerParamCallback(nuint context, nuint track_vars_array, ServerParamRetnCallback retn) {
      try {
        ((PageSapi)CaseScript[context]).ServerParamCallback((string key, string value) => {
          retn(track_vars_array, key, value);
        });
      } catch { }
    }


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ExecuteThreadIdCallback(nuint context);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ServerParamRetnCallback(nuint track_vars_array, string key, string val);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SendHeader(nuint context, string head, long str_length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe public delegate long UbWrite(nuint context, byte* str, long str_length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int Flush(nuint context);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe public delegate long ReadPost(nuint context, byte* buf, long count_bytes);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate string ReadCookies(nuint context);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ServerParam(nuint context, nuint track_vars_array, ServerParamRetnCallback retn);


    [DllImport("libxphp", CallingConvention = CallingConvention.Cdecl)]
    public static extern nuint tsrm_thread_id();
    [DllImport("libxphp", CallingConvention = CallingConvention.Cdecl)]
    public static extern int On(
      SendHeader sendHeader,
      UbWrite write,
      Flush flush,
      ReadPost readPost,
      ReadCookies readCookies,
      ServerParam serverParam);
    [DllImport("libxphp", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Off();
    [DllImport("libxphp", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Execute(nuint context, string method, string content_type, long content_length, int filename_index, string filepath, string query_string);
  }
}