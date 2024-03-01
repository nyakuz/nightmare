using XSAPI;

namespace Nightmare.Main {
  public class VirtualScriptWatcher: LazyFileSystemWatcher {
    public struct ScriptInfo {
      public Module TargetModule;
      public string RequestFilePath;
      public FileInfo File;

      public ScriptInfo(Module module, string request_filepath, FileInfo file) {
        TargetModule = module;
        RequestFilePath = request_filepath;
        File = file;
      }
    }

    public string Hostname;
    public readonly MainRouter Router;
    public readonly int VirtualHostPathLength;
    public VirtualScriptWatcher(MainRouter router, int vhost_path_length, string hostname, string path) : base(path) {
      var filter = new HashSet<string>();
      foreach(var (_, module) in router.ModuleExtension) {
        foreach(var ext in module.FileWatchExtension) {
          filter.Add(ext);
        }
      }

      Router = router;
      Hostname = hostname;
      VirtualHostPathLength = vhost_path_length + hostname.Length;
      Path = path;

      NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
      Filter = string.Join(';', filter);
      IncludeSubdirectories = true;
      Error += ScriptWatcher_Error;
      LazyCreated += ScriptWatcher_Created;
      LazyChanged += ScriptWatcher_Changed;
      LazyRenamed += ScriptWatcher_Renamed;
      LazyDeleted += ScriptWatcher_Deleted;
      
      Update();
    }

    public void Reset(string hostname, string path) {
      EnableRaisingEvents = false;
      Hostname = hostname;
      Path = path;
      EnableRaisingEvents = true;
    }

    private void ScriptWatcher_Error(object sender, ErrorEventArgs e) {
      Console.Error.WriteLine(e.GetException().Message);

      Update();
    }

    private void ScriptWatcher_Created(object sender, FileSystemEventArgs e) {
      if(e.Name == null) return;

      var file_extension = System.IO.Path.GetExtension(e.Name!);

      foreach(var (_, module) in Router.ModuleExtension) {
        if(module.ScriptFileExtension.Contains(file_extension) == false) continue;

        module.InvokeEvent(Module.EventName.FileSystemWatcher, Module.EventMethod.CreateScript, Hostname, e.FullPath[VirtualHostPathLength..^4], e);
      }
    }

    private void ScriptWatcher_Changed(object sender, FileSystemEventArgs e) {
      if(e.Name == null) return;

      var file_extension = System.IO.Path.GetExtension(e.Name!);

      foreach(var (_, module) in Router.ModuleExtension) {
        if(module.ScriptFileExtension.Contains(file_extension) == false) continue;

        module.InvokeEvent(Module.EventName.FileSystemWatcher, Module.EventMethod.ChangeScript, Hostname, e.FullPath[VirtualHostPathLength..^4], e);
      }
    }

    private void ScriptWatcher_Renamed(object sender, RenamedEventArgs e) {
      if(e.Name == null) return;
      
      var file_extension = System.IO.Path.GetExtension(e.Name!);

      foreach(var (_, module) in Router.ModuleExtension) {
        if(module.ScriptFileExtension.Contains(file_extension) == false) continue;

        module.InvokeEvent(Module.EventName.FileSystemWatcher, Module.EventMethod.RenameScript, Hostname, e.FullPath[VirtualHostPathLength..^4], e.OldFullPath[VirtualHostPathLength..^4], e);
      }
    }

    private void ScriptWatcher_Deleted(object sender, FileSystemEventArgs e) {
      if(e.Name == null) return;

      var file_extension = System.IO.Path.GetExtension(e.Name!);

      foreach(var (_, module) in Router.ModuleExtension) {
        if(module.ScriptFileExtension.Contains(file_extension) == false) continue;

        module.InvokeEvent(Module.EventName.FileSystemWatcher, Module.EventMethod.DeleteScript, Hostname, e.FullPath[VirtualHostPathLength..^4], e);
      }
    }

    public void Update() {
      EnableRaisingEvents = false;

      var di = new DirectoryInfo(Path);
      var li = new List<ScriptInfo>();
      var li2 = new List<ScriptInfo>();
      
      ScanDirectoryAll(di, li);

      foreach(var info in li) {
        UpdateFile(info.TargetModule, info.RequestFilePath, info.File, li2);
      }

      EnableRaisingEvents = true;
    }

    public void UpdateFile(Module module, string request_filepath, FileInfo file, List<ScriptInfo> twopass) {
      var code = module.InvokeEvent(Module.EventName.FileSystemWatcher, Module.EventMethod.ResetScript, Hostname, request_filepath, file);
      if(code != 0) {
        twopass.Add(new(module, request_filepath, file));
      }
    }

    public IEnumerable<DirectoryInfo> ScanDirectory(DirectoryInfo directory_info, List<ScriptInfo> twopass) {
      foreach(var file in directory_info.EnumerateFiles()) {
        var request_filepath = file.FullName[VirtualHostPathLength..^4];

        foreach(var (_, module) in Router.ModuleExtension) {
          if(module.ScriptFileExtension.Contains(file.Extension) == false) continue;

          UpdateFile(module, request_filepath, file, twopass);
        }
      }
      return directory_info.EnumerateDirectories();
    }

    public void ScanDirectoryAll(DirectoryInfo directory_info, List<ScriptInfo> twopass) {
      foreach(var di in ScanDirectory(directory_info, twopass)) {
        ScanDirectoryAll(di, twopass);
      }
    }
  }
}