using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Collections.Concurrent;
using XSAPI;

namespace XCsModule {
  public class XCsScript : Module {
    public static IEnumerable<string> IndexFileList = ["index"];
    public static IEnumerable<string> FileExtensionList = [".csx"];
    public static readonly AssemblyResolver Resolver = new();
    public static readonly ScriptBuilder Builder;
    static readonly ConcurrentDictionary<string, ScriptContext> VirtualHostContext = new();


    static XCsScript() {
      var option = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
#if DEBUG
        .WithOptimizationLevel(OptimizationLevel.Debug)
#else
        .WithOptimizationLevel(OptimizationLevel.Release)
#endif
        .WithConcurrentBuild(true)
        .WithPlatform(Platform.AnyCpu)
        .WithMetadataImportOptions(MetadataImportOptions.All)
        .WithNullableContextOptions(NullableContextOptions.Enable)
        .WithMetadataReferenceResolver(Resolver)
        .WithUsings(
          "System",
          "System.IO",
          "System.Collections.Generic",
          "System.Console",
          "System.Diagnostics",
          "System.Dynamic",
          "System.Linq",
          "System.Linq.Expressions",
          "System.Text",
          "System.Threading.Tasks");


      var reference = CSharpScript.Create(string.Empty).GetCompilation().References;
      var app_path = Path.GetDirectoryName(typeof(Module).Assembly.Location)!;
      var netcore_path = Path.GetDirectoryName(typeof(GC).Assembly.Location);
      var aspnetcore_path = netcore_path!.Replace("Microsoft.NETCore", "Microsoft.AspNetCore");

      Builder = new ScriptBuilder(option, reference, app_path, netcore_path, aspnetcore_path);
    }
    public static void AddMetadataReference(string filepath) {
      Builder.Reference.Add(MetadataReference.CreateFromFile(filepath));
    }


    public override IEnumerable<string> ScriptFileExtension { get => FileExtensionList; }
    public override IEnumerable<string> FileWatchExtension { get {
        foreach(var ext in FileExtensionList) {
          yield return "*" + ext;
        }
    } }
    public override ValueTask<int> InvokePage(PageSapi sapi) {
      return VirtualHostContext[sapi.VirtualHostName].InvokePage(sapi);
    }

    public override ValueTask<int> InvokeHubConnected(IHub hub) {
      return VirtualHostContext[hub.VirtualHostName].InvokeHubConnected(hub);
    }
    public override ValueTask<int> InvokeHubDisconnected(IHub hub) {
      return VirtualHostContext[hub.VirtualHostName].InvokeHubDisconnected(hub);
    }
    public override ValueTask<int> InvokeHubDataReceived(IHub hub, string req_path, params dynamic[] args) {
      return VirtualHostContext[hub.VirtualHostName].InvokeHubDataReceived(hub, req_path, args);
    }

    public override int InvokeEvent(EventName event_name, EventMethod event_method, string hostname, params dynamic[] arguments) {
      switch(event_name) {
      case EventName.FileSystemWatcher:
        switch(event_method) {
        case EventMethod.CreateVirtualHost:
          VirtualHostContext[hostname] = new();
          break;
        case EventMethod.RenameVirtualHost:
          VirtualHostContext[hostname] = VirtualHostContext[(string)arguments[0]];
          VirtualHostContext.TryRemove((string)arguments[0], out _);
          break;
        case EventMethod.DeleteVirtualHost:
          VirtualHostContext.TryRemove(hostname, out _);
          break;
        case EventMethod.ResetScript:
          if(VirtualHostContext[hostname].On(arguments[0], ((FileInfo)arguments[1]).FullName) == false) {
            return -1;
          }
          break;
        case EventMethod.CreateScript:
          if(VirtualHostContext[hostname].On(arguments[0], ((FileSystemEventArgs)arguments[1]).FullPath) == false) {
            return -1;
          }
          break;
        case EventMethod.RenameScript:
          VirtualHostContext[hostname].Off(arguments[1]);
          if(VirtualHostContext[hostname].On(arguments[0], ((FileSystemEventArgs)arguments[2]).FullPath) == false) {
            return -1;
          }
          break;
        case EventMethod.ChangeScript:
          switch(((FileSystemEventArgs)arguments[1]).ChangeType) {
          case WatcherChangeTypes.Changed:
            if(VirtualHostContext[hostname].On(arguments[0], ((FileSystemEventArgs)arguments[1]).FullPath) == false) {
              return -1;
            }
            break;
          }
          break;
        case EventMethod.DeleteScript:
          VirtualHostContext[hostname].Off(arguments[0]);
          break;
        }
        break;
      }

      return 0;
    }
  }
}