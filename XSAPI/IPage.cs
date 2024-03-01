using XSAPI;

public interface IPage : IContext, ISecureCookie {
  public string? GetQuery(string name);
}