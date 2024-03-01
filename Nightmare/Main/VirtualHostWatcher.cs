using System.Collections.Concurrent;
using XSAPI;

namespace Nightmare.Main {
  public class VirtualHostWatcher : ConcurrentDictionary<string, VirtualScriptWatcher> {
    const string EmptyExtension = "";
    const string TextExtension = ".txt";
    const string BackupExtension = ".bak";
    public readonly ConcurrentDictionary<string, string> OverrideHostname = new();
    readonly LazyFileSystemWatcher VirtualHost = new();
    readonly MainRouter Router;

    public VirtualHostWatcher(MainRouter router) {
      Router = router;

      VirtualHost.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
      VirtualHost.IncludeSubdirectories = false;
      VirtualHost.Error += VirtualHost_Error;
      VirtualHost.LazyCreated += VirtualHostWatcher_Created;
      VirtualHost.LazyChanged += VirtualHostWatcher_Changed;
      VirtualHost.LazyRenamed += VirtualHostWatcher_Renamed;
      VirtualHost.LazyDeleted += VirtualHostWatcher_Deleted;
    }

    private void VirtualHost_Error(object sender, ErrorEventArgs e) {
      Console.Error.WriteLine(e.GetException().Message);

      Update(VirtualHost.Path);
    }

    private void VirtualHostWatcher_Created(object sender, FileSystemEventArgs e) {
      if(e.Name == null) return;

      switch(Path.GetExtension(e.Name)) {
      case TextExtension:
        VirtualHostOverrideCreate(e.Name!, e.FullPath);
        break;
      case EmptyExtension:
        VirtualHostCreate(VirtualHost.Path.Length, e.Name!, e.FullPath);
        break;
      }
    }

    private async void VirtualHostWatcher_Renamed(object sender, RenamedEventArgs e) {
      string vhostname;

      if(e.Name == null) return;

      switch(Path.GetExtension(e.Name)) {
      case TextExtension:
        vhostname = Path.GetFileNameWithoutExtension(e.Name!);

        using(var sr = File.OpenText(e.OldFullPath + BackupExtension)) {
          string? line;

          while((line = await sr.ReadLineAsync()) != null) {
            OverrideHostname.TryRemove(line, out var _);
          }
        }

        using(var sr = File.OpenText(e.FullPath)) {
          string? line;

          while((line = await sr.ReadLineAsync()) != null) {
            OverrideHostname[line] = vhostname;
          }
        }

        File.Move(e.OldFullPath + BackupExtension, e.FullPath + BackupExtension, true);
        break;
      case EmptyExtension:
        var hostname = e.Name!;
        var hostname_old = e.OldName!;
        var fsw = this[hostname_old];

        fsw.Reset(hostname, e.FullPath);
        this[hostname] = fsw;
        TryRemove(hostname, out _);

        foreach(var (_, module) in Router.ModuleExtension) {
          module.InvokeEvent(Module.EventName.FileSystemWatcher, Module.EventMethod.RenameVirtualHost, hostname, hostname_old);
        }

        break;
      }
    }

    async private void VirtualHostWatcher_Deleted(object sender, FileSystemEventArgs e) {
      if(e.Name == null) return;

      switch(Path.GetExtension(e.Name)) {
      case TextExtension:
        using(var sr = File.OpenText(e.FullPath + BackupExtension)) {
          string? line;

          while((line = await sr.ReadLineAsync()) != null) {
            OverrideHostname.TryRemove(line, out var _);
          }
        }

        File.Delete(e.FullPath + BackupExtension);

        break;
      case EmptyExtension:
        TryRemove(e.Name!, out _);

        foreach(var (_, module) in Router.ModuleExtension) {
          module.InvokeEvent(Module.EventName.FileSystemWatcher, Module.EventMethod.DeleteVirtualHost, e.Name!);
        }

        break;
      }
    }

    async private void VirtualHostWatcher_Changed(object sender, FileSystemEventArgs e) {
      string vhostname;

      if(e.Name == null) return;

      switch(Path.GetExtension(e.Name)) {
      case TextExtension:
        vhostname = Path.GetFileNameWithoutExtension(e.Name!);
        
        using(var sr = File.OpenText(e.FullPath)) {
          string? line;

          while((line = await sr.ReadLineAsync()) != null) {
            OverrideHostname[line] = vhostname;
          }
        }

        File.Copy(e.FullPath, e.FullPath + BackupExtension, true);

        break;
      }
    }


    public void Update(string path) {
      VirtualHost.EnableRaisingEvents = false;
      VirtualHost.Path = path;

      var vhost_path_length = VirtualHost.Path.Length;
      var di = new DirectoryInfo(path);
      foreach(var dir in di.EnumerateDirectories()) {
        if(dir.Exists == false) continue;

        VirtualHostCreate(vhost_path_length, dir.Name, dir.FullName);
      }

      foreach(var file in di.EnumerateFiles("*" + TextExtension)) {
        switch(file.Extension) {
        case TextExtension:
          VirtualHostOverrideCreate(file.Name, file.FullName);
          break;
        }
      }

      VirtualHost.EnableRaisingEvents = true;
    }

    private void VirtualHostCreate(int vhost_path_length, string hostname, string filepath) {
      foreach(var (_, module) in Router.ModuleExtension) {
        module.InvokeEvent(Module.EventName.FileSystemWatcher, Module.EventMethod.CreateVirtualHost, hostname);
      }

      this[hostname] = new VirtualScriptWatcher(Router, vhost_path_length, hostname, filepath);
    }

    private async void VirtualHostOverrideCreate(string filename, string filepath) {
      var vhostname = Path.GetFileNameWithoutExtension(filename);

      using(var sr = File.OpenText(filepath)) {
        string? line;

        while((line = await sr.ReadLineAsync()) != null) {
          OverrideHostname[line] = vhostname;
        }
      }

      File.Copy(filepath, filepath + BackupExtension, true);
    }
  }
}
