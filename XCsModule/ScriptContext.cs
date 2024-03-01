using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using XSAPI;

namespace XCsModule {
  public class ScriptContext : ConcurrentDictionary<string, ScriptReference> {
    public bool On(string request_path, string script_filepath) {
      bool result;
      try {
        result = XCsScript.Builder.Build(this, script_filepath, out var script, out var metadata, out var assembly);

        if(result == true) {
          var typename = Path.GetFileNameWithoutExtension(request_path);
          script!.Resolving += Script_Resolving;

          var item = new ScriptReference(typename, script!, metadata!, assembly!);
          this[request_path] = item;

          foreach(var index_filename in XCsScript.IndexFileList) {
            if(request_path.EndsWith(index_filename) == true) {
              this[Path.GetDirectoryName(request_path)!] = item;
              break;
            }
          }
        }
      }catch(Exception ex) { 
        Console.Error.WriteLine(ex);
        result = false;
      }

      return result;
    }

    private Assembly Script_Resolving(AssemblyLoadContext context, AssemblyName assembly_name) {
      return this[assembly_name.Name!].Assembly;
    }

    public void Off(string request_path) {
      TryRemove(request_path, out var _);
      foreach(var index_filename in XCsScript.IndexFileList) {
        if(request_path.EndsWith(index_filename) == true) {
          TryRemove(Path.GetDirectoryName(request_path)!, out var _);
          break;
        }
      }
    }

    public ValueTask<int> InvokePage(PageSapi sapi) {
      return this[sapi.RequestPath].InvokePage(sapi);
    }

    public ValueTask<int> InvokeHubConnected(IHub hub) {
      return this[hub.RequestPath].InvokeHubConnected(hub);
    }

    public ValueTask<int> InvokeHubDisconnected(IHub hub) {
      return this[hub.RequestPath].InvokeHubDisconnected(hub);
    }

    public ValueTask<int> InvokeHubDataReceived(IHub hub, string req_path, params dynamic[] args) {
      return this[req_path].InvokeHubDataReceived(hub, args);
    }
  }
}
