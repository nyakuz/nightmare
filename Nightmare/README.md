# Nightmare Web Server

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
- localhost.txt (test.localhost\r\napi.localhost)
- example.com.txt (www.example.com\r\napi.example.com)
```

## Installation

[libxphp](/libxphp/README.md)
[Building Dependency Packages](/)

This command builds a Docker image named 'nightmare' using the Dockerfile located in the 'Nightmare' directory for the AspNetCore.Nightmare application.

    docker build -f "Nightmare/Dockerfile" --force-rm -t nightmare AspNetCore.Nightmare

This command is an example of running in rootless mode.

    docker run --name Website1 -d --restart always -p 80:8080 -p 8443:443/udp -p 8443:443 /home/ubuntu/app/appsettings.json:/app/appsettings.json -v /home/ubuntu/app/kestrel.json:/app/kestrel.json -v /home/ubuntu/app/config:/app/config -v /home/ubuntu/vhost:/app/vhost/ nightmare

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
