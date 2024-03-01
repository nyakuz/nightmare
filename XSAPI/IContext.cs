namespace XSAPI {
  public interface IContext {
    public string VirtualHostName { get; }
    public string RequestPath { get; }

    public dynamic? GetProperty(Type method_type);
    public dynamic? GetService(Type method_type);
  }
}