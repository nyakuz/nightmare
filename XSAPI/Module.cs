namespace XSAPI {
  public abstract class Module {
    public enum EventName {
      FileSystemWatcher
    }
    public enum EventMethod {
      CreateVirtualHost,
      RenameVirtualHost,
      ChangeVirtualHost,
      DeleteVirtualHost,
      ResetScript,
      CreateScript,
      RenameScript,
      ChangeScript,
      DeleteScript
    }
    public abstract IEnumerable<string> ScriptFileExtension { get; }
    public abstract IEnumerable<string> FileWatchExtension { get; }
    public abstract ValueTask<int> InvokePage(PageSapi sapi);
    public abstract ValueTask<int> InvokeHubConnected(IHub hub);
    public abstract ValueTask<int> InvokeHubDisconnected(IHub hub);
    public abstract ValueTask<int> InvokeHubDataReceived(IHub hub, string req_path, params dynamic[] args);
    public abstract int InvokeEvent(EventName event_name, EventMethod event_method, string hostname, params dynamic[] arguments);
  }
}
