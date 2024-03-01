
namespace XSAPI {
  public abstract class PageSapi : Sapi {
    public PageSapi() {
      Type = SapiType.Page;
    }

    public abstract bool HasStarted { get; }

    public abstract dynamic? GetProperty(Type method_type);
    public abstract dynamic? GetService(Type service_type);
    public abstract string? GetQuery(string name);

    public abstract void GetParameter(out string method, out string content_type, out long content_length, out string query_string);
    public abstract void SetHttpState(int code);

    public abstract ValueTask<string> ReadBodyTextPlain();
    public abstract Task ResponseFile(string filepath, string mime_type);

    public abstract void SendHeaderCallback(string name, string value);
    public abstract long UbWriteCallback(ReadOnlySpan<byte> str, long str_length);
    public abstract ValueTask<int> SendFlushCallback();
    public abstract long ReadPostCallback(Span<byte> buf, long count_bytes);
    public abstract string ReadCookiesCallback();
    public abstract void ServerParamCallback(Action<string, string> retn);
  }
}
