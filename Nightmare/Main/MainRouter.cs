using System.Collections.Concurrent;
using System.Configuration;
using XSAPI;

namespace Nightmare.Main {
  public class MainRouter {
    public const string DefaultVhostDirectory = "vhost/";
    public const string DefaultAnyhostDirectory = "anyhost/";

    public string VhostDirectory;
    public readonly ModuleContext Modules = new();
    public readonly ConcurrentDictionary<string, Module> ModuleExtension = new();
    public readonly HashSet<string> FileExtensionFilter = new();
    public readonly VirtualHostWatcher VirtualHost;
    public string[] IndexFileExtensions;
    public MainRouter(IConfiguration config, IServiceCollection services, string content_root_path) {
      VhostDirectory = Path.Combine(content_root_path, DefaultVhostDirectory);
      VirtualHost = new(this);
      IndexFileExtensions = config.GetValue<string[]>("AppSettings.IndexFileExtensions", [".html",".htm"])!;

      foreach(var file in new DirectoryInfo(Path.Combine(content_root_path, "Modules")).GetFiles()) {
        if(file.Name.StartsWith("X") == false || file.Name.EndsWith(".dll") == false) continue;
        Console.WriteLine("LoadModule: {0}", file.FullName);
        Add(Modules.LoadFromAssemblyPath(file.Name, file.FullName));
      }

      services.AddScoped<IMysqlService, MysqlService>();

      //Add(new XPhpModule.XPhpScript());
      //Add(new XCsModule.XCsScript());

      Update();
    }


    public void Add(Module module) {
      foreach(var file_extension in module.ScriptFileExtension) {
        ModuleExtension[file_extension] = module;
        FileExtensionFilter.Add(file_extension);
      }
    }

    public void Update() {
      VirtualHost.Update(VhostDirectory);
    }

    public string FindVirtualHost(string hostname) {
      if(VirtualHost.OverrideHostname.TryGetValue(hostname, out var host) == true) {
        return host;
      }

      var vhostname = hostname;

      if(VirtualHost.ContainsKey(vhostname) == true) {
        return vhostname;
      }

      return string.Empty;
    }

    public async Task InvokePage(MainMiddleware.PageSapiContext sapi) {
      var vhostname = FindVirtualHost(sapi.RequestHost);

      if(vhostname == string.Empty) {
        return;
      }

      var req_path = sapi.RequestPath;
      var fileext = Path.GetExtension(req_path).ToLower();
      var ignore_static_file = FileExtensionFilter.Contains(fileext);

      switch(req_path) {
      case "/":
        req_path = "/index";
        break;
      }

      for(var two_pass = 2; two_pass>0; two_pass--) {
        var vpath_dir = VhostDirectory + vhostname;
        var vhost_fullpath = vpath_dir + req_path;

        if(ignore_static_file == false) {
          if(File.Exists(vhost_fullpath) == true) {
            await sapi.ResponseFile(vhost_fullpath, fileext);
            return;
          }
          foreach(var ext in IndexFileExtensions) {
            if(File.Exists(vhost_fullpath + ext) == true) {
              await sapi.ResponseFile(vhost_fullpath + ext, ext);
              return;
            }
          }
        }

        sapi.VhostName = vhostname;

        foreach(var (file_extension, module) in ModuleExtension) {
          var tmp_extension = req_path.EndsWith(file_extension) == true ? string.Empty : file_extension;
          var tmp = vhost_fullpath + tmp_extension;

          if(File.Exists(tmp) == true) {
            var vhost_filepath2 = req_path + tmp_extension;

            sapi.SetVhostPath(vpath_dir, vhost_filepath2, tmp);
            await module.InvokePage(sapi);

            return;
          }
        }

        foreach(var (file_extension, module) in ModuleExtension) {
          var tmp_extension = req_path.EndsWith(file_extension) == true ? string.Empty : file_extension;
          var vhost_router = "/router" + file_extension;
          var tmp = vpath_dir + vhost_router;

          if(File.Exists(tmp) == true) {
            sapi.SetVhostPath(vpath_dir, vhost_router, tmp);
            var code = await module.InvokePage(sapi);

            switch(code) {
            case 1: continue;
            default:
              if(sapi.HasStarted == false && code > 0 && code < 1000) {
                sapi.SetHttpState(code);
              }
              return;
            }
          }
        }

        vhostname = DefaultAnyhostDirectory[..^1];
      }
    }

    public async Task<int> InvokeHubConnected(MainHub hub, HubSapi sapi, string req_path) {
      var vpath_dir = VhostDirectory + sapi.VirtualHostName;
      var vhost_fullpath = vpath_dir + req_path;

      foreach(var (file_extension, module) in ModuleExtension) {
        var tmp = vhost_fullpath + file_extension;

        if(File.Exists(tmp) == true) {
          var vhost_filepath2 = req_path + file_extension;

          sapi.SetVhostPath(vpath_dir, vhost_filepath2, tmp);
          await module.InvokeHubConnected(hub);

          return 0;
        }
      }

      return 500;
    }

    public async Task<int> InvokeHubDisconnected(MainHub hub, HubSapi sapi, string req_path) {
      var vpath_dir = VhostDirectory + sapi.VirtualHostName;
      var vhost_fullpath = vpath_dir + req_path;

      foreach(var (file_extension, module) in ModuleExtension) {
        var tmp = vhost_fullpath + file_extension;

        if(File.Exists(tmp) == true) {
          var vhost_filepath2 = req_path + file_extension;

          sapi.SetVhostPath(vpath_dir, vhost_filepath2, tmp);
          await module.InvokeHubDisconnected(hub);

          return 0;
        }
      }

      return 500;
    }

    public async Task<int> InvokeHubDataReceived(MainHub hub, string req_path, params dynamic[] args) {
      var sapi = hub.Sapi;
      var vpath_dir = VhostDirectory + sapi.VirtualHostName;
      var vhost_fullpath = vpath_dir + req_path;

      foreach(var (file_extension, module) in ModuleExtension) {
        var tmp = vhost_fullpath + file_extension;

        if(File.Exists(tmp) == true) {
          var vhost_filepath2 = req_path + file_extension;

          sapi.SetVhostPath(vpath_dir, vhost_filepath2, tmp);
          return await module.InvokeHubDataReceived(hub, req_path, args);
        }
      }

      return 404;
    }
  }
}
