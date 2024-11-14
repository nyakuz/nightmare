# Nightmare Web Server

## Build the Dockerfile.

This command builds a Docker image named 'nightmare' using the Dockerfile located in the 'Nightmare' directory for the AspNetCore.Nightmare application.

```console
docker build -f "Nightmare/Dockerfile" --force-rm -t nightmare AspNetCore.Nightmare
```

This command is an example of running in rootless mode.

```console
docker run --name Website1 -d --restart always -p 80:8080 -p 8443:443/udp -p 8443:443 /home/ubuntu/app/appsettings.json:/app/appsettings.json -v /home/ubuntu/app/config:/app/config -v /home/ubuntu/vhost:/app/vhost/ nightmare
```

## Native Debug

```console
docker run --name Website1 -d -p 80:8080 -p 8443:443/udp -p 8443:443 --cap-add=NET_ADMIN --cap-add=SYS_PTRA-security-opt seccomp=unconfined /home/ubuntu/app/appsettings.json:/app/appsettings.json -v /home/ubuntu/app/kestrel.json:/app/kestrel.json -v /home/ubuntu/app/config:/app/config -v /home/ubuntu/vhost:/app/vhost/ nightmare bash
apt clean && apt update
apt install -y gdb
gdb --args dotnet Nightmare.dll
```


## Issues

### Curl Http3 Memory Leak (nghttp3)

The problem is very serious.
There is a memory leak in the HTTP3 implementation of curl, which is most likely caused by the curl_exec function.

| Name | Branch | Version | Filename |
| --- | --- | --- | --- |
| OpenSSL+quictls | [openssl-3.3.0-quic1](https://github.com/quictls/openssl/tree/openssl-3.3.0-quic1) | 3.3.0-quic1 | openssl.deb |
| NgHttp2 | [v1.64.0](https://github.com/nghttp2/nghttp2/tree/v1.64.0) | 1.64.0 | libnghttp2.deb |
| NgHttp3 | [v1.6.0](https://github.com/ngtcp2/nghttp3/tree/v1.6.0) | 1.6.0 | libnghttp3.deb |
| NgTcp2 | [v1.8.1](https://github.com/ngtcp2/ngtcp2/tree/v1.8.1) | 1.8.1 | libngtcp2.deb |

### Loading libphp module issue in .NET

Note for .NET
There is a bug with static loading of dynamic libraries in .NET, which requires this approach to be taken.

php-error.log

	PHP Warning:  PHP Startup: Unable to load dynamic library 'curl' no-debug-zts-20220829/curl.so: undefined symbol: core_globals_offset in Unknown on line 0

To address this issue, utilize the following command:

	LD_PRELOAD="${LD_PRELOAD}:/usr/local/lib/libphp.so"
