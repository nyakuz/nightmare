using System.Collections.Concurrent;

namespace Nightmare.Main {
  public class CancelableTaskQueue<T> : ConcurrentDictionary<T, CancellationTokenSource> where T : notnull {
    struct Info {
      public Action<T> Procedure;
      public T Data;
      public Info(Action<T> action, T data) {
        Procedure = action;
        Data = data;
      }
    }
    readonly Queue<Info> queue = new();

    public async ValueTask Invoke(Action<T> action, T data, int millisecondsDelay) {
      if(TryGetValue(data, out  var cts2) == true && cts2.IsCancellationRequested == false) {
        cts2.Cancel();
      }

      var cts = new CancellationTokenSource();
      this[data] = cts;

      try {
        await Task.Delay(millisecondsDelay, cts.Token);
        queue.Enqueue(new Info(action,data));

        if(queue.Count == 0) {
          await Task.Run(Execute);
        }
      } catch(TaskCanceledException) { }
    }

    public void Cancel(T data) {
      TryRemove(data, out _);
    }

    private void Execute() {
      while(queue.TryDequeue(out var info)) {
        info.Procedure(info.Data);

        TryRemove(info.Data, out _);
      }
    }
  }
}
