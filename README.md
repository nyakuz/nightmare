# Nightmare Web Server

## Description

Nightmare is a web server based on AspNetCore that supports the latest protocols and scripting languages. Currently, it supports PHP and CSX scripts.

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
