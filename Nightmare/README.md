# Nightmare Web Server

## Build the Dockerfile.

[libxphp](/libxphp/README.md)
[Building Dependency Packages](/)

This command builds a Docker image named 'nightmare' using the Dockerfile located in the 'Nightmare' directory for the AspNetCore.Nightmare application.

    docker build -f "Nightmare/Dockerfile" --force-rm -t nightmare AspNetCore.Nightmare

This command is an example of running in rootless mode.

    docker run --name Website1 -d --restart always -p 80:8080 -p 8443:443/udp -p 8443:443 /home/ubuntu/app/appsettings.json:/app/appsettings.json -v /home/ubuntu/app/kestrel.json:/app/kestrel.json -v /home/ubuntu/app/config:/app/config -v /home/ubuntu/vhost:/app/vhost/ nightmare

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

## Error Fix (important)

### Unable to Configure Kestrel

Note for .NET
The Kestrel options are not being applied in the appsettings.json configuration file, so the kestrel.json configuration file needs to be specified.
In rootless mode, ports 8080 and 8443 can be used, while in root mode, ports 80 and 443 can be used.

### Error PHP Module

Note for .NET
There is a bug with static loading of dynamic libraries in .NET, which requires this approach to be taken.

    PHP Warning:  PHP Startup: Unable to load dynamic library 'curl' no-debug-zts-20220829/curl.so: undefined symbol: core_globals_offset in Unknown on line 0

To address this issue, utilize the following command:

    LD_PRELOAD "${LD_PRELOAD}:/usr/local/lib/libphp.so"
