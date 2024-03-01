# Nightmare Web Server

## Description

Nightmare is a web server based on AspNetCore that supports the latest protocols and scripting languages. Currently, it supports PHP and CSX scripts.

## Quick start

```shell
docker run --name Website1 -d -p 80:8080 -p 8443:443/udp -p 8443:443 \
  -v <VHOST_DIR>:/app/vhost \
  nyakuz/nightmare:latest
```

An example of a vhost path.
```
<VHOST_DIR>/
- anyhost
- localhost
-- index.php
-- test.csx
-- robots.txt
-- favicon.ico
- example.com/
-- ..
- localhost.txt (www.example.com\r\napi.example.com)
```

## Direct load modules
Add modules to load from Nightmare > dependencies > projects.

/Main/MainRouter.cs
```cs
public MainRouter(IServiceCollection services, string content_root_path) {
    VhostDirectory = Path.Combine(content_root_path, DefaultVhostDirectory);
    VirtualHost = new(this);

    foreach(var file in new DirectoryInfo(Path.Combine(content_root_path, "Modules")).GetFiles()) {
        if(file.Name.StartsWith("X") == false || file.Name.EndsWith(".dll") == false) continue;
        Console.WriteLine("LoadModule: {0}", file.FullName);
        //Add(Modules.LoadFromAssemblyPath(file.Name, file.FullName));
    }

    services.AddScoped<IMysqlService, MysqlService>();

    Add(new XPhpModule.XPhpScript()); // PHP
    Add(new XCsModule.XCsScript()); // CS

    Update();
}
```
