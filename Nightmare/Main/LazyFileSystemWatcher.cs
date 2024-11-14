using System.Collections.Concurrent;

namespace Nightmare.Main {
  public class LazyFileSystemWatcher : FileSystemWatcher {
    static ConcurrentDictionary<string, CancellationTokenSource> TaskToken = new();
    enum EventType {
      Create,
      Change,
      Rename,
      Delete
    }
    struct QueueInfo {
      public string FullPath;
      public EventType Type;
      public dynamic EventArg;
      public QueueInfo(string fullpath, EventType event_type, dynamic arg) {
        FullPath = fullpath;
        Type = event_type;
        EventArg = arg;
      }
    }

    readonly Queue<QueueInfo> TaskQueue = new();
    public int LazyTime = 0;
    public event FileSystemEventHandler? LazyCreated;
    public event FileSystemEventHandler? LazyChanged;
    public event RenamedEventHandler? LazyRenamed;
    public event FileSystemEventHandler? LazyDeleted;

    public LazyFileSystemWatcher(int lazytime = 1000) : base() {
      LazyTime = lazytime;
      Created += OnCreated;
      Changed += OnChanged;
      Renamed += OnRenamed;
      Deleted += OnDeleted;
    }
    public LazyFileSystemWatcher(string path, int lazytime = 1000) : base(path) {
      LazyTime = lazytime;
      Created += OnCreated;
      Changed += OnChanged;
      Renamed += OnRenamed;
      Deleted += OnDeleted;
    }

    private void OnCreated(object sender, FileSystemEventArgs e) {
      Invoke(e.FullPath, EventType.Create, e, LazyTime);
    }
    private void OnRenamed(object sender, RenamedEventArgs e) {
      Invoke(e.FullPath, EventType.Rename, e, LazyTime);
    }
    private void OnDeleted(object sender, FileSystemEventArgs e) {
      Invoke(e.FullPath, EventType.Delete, e, LazyTime);
    }
    private void OnChanged(object sender, FileSystemEventArgs e) {
      Invoke(e.FullPath, EventType.Change, e, LazyTime);
    }

    private async void Invoke(string fullpath, EventType type, dynamic e, int millisecondsDelay) {
      if(TaskToken.TryGetValue(fullpath, out var cts2) == true && cts2.IsCancellationRequested == false) {
        cts2.Cancel();
      }

      var cts = new CancellationTokenSource();
      TaskToken[fullpath] = cts;

      try {
        await Task.Delay(millisecondsDelay, cts.Token);

        TaskToken.TryRemove(fullpath, out _);
        TaskQueue.Enqueue(new QueueInfo(fullpath, type, e));

        if(TaskQueue.Count > 0) {
          await Task.Factory.StartNew(Execute);
        }
      } catch(TaskCanceledException) { }
    }

    private void Cancel(string path) {
      TaskToken.TryRemove(path, out _);
    }

    private void Execute() {
      while(TaskQueue.TryDequeue(out var info)) {
        switch(info.Type) {
        case EventType.Create: LazyCreated?.Invoke(this, info.EventArg); break;
        case EventType.Delete: LazyDeleted?.Invoke(this, info.EventArg); break;
        case EventType.Rename: LazyRenamed?.Invoke(this, info.EventArg); break;
        case EventType.Change: LazyChanged?.Invoke(this, info.EventArg); break;
        }
      }
    }
  }
}