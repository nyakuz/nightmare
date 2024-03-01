using XSAPI;

public interface IHub : IContext, ISecureCookie {
  public void SendCaller(string method, params object[] arg);
  public void SendClient(string connection_id, string method, params object[] arg);
}