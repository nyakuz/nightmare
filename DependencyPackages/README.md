# Build Package

| Name | Branch | Version | Filename |
| --- | --- | --- | --- |
| OpenSSL+quictls | [openssl-3.3.0-quic1](https://github.com/quictls/openssl/tree/openssl-3.3.0-quic1) | 3.3.0-quic1 | openssl.deb |
| libssl3 | --- | --- | libssl3.deb |
| libssl-dev | --- | --- | libssl-dev.deb |
| NgHttp2 | [v1.64.0](https://github.com/nghttp2/nghttp2/tree/v1.64.0) | 1.64.0 | libnghttp2.deb |
| NgHttp3 | [v1.6.0](https://github.com/ngtcp2/nghttp3/tree/v1.6.0) | 1.6.0 | libnghttp3.deb |
| NgTcp2 | [v1.8.1](https://github.com/ngtcp2/ngtcp2/tree/v1.8.1) | 1.8.1 | libngtcp2.deb |
| Curl | [curl-8_11_0](https://github.com/curl/curl/tree/curl-8_11_0) | 8.11.0 | curl.deb |
| Php | [php-8.3.13](https://github.com/php/php-src/tree/php-8.3.13) | 8.3.13 | php.deb |


## 1. Download Source code
Create or mount the /src directory.

Curl, Php recommends downloading the official release of the source file.
[curl](https://curl.se/download.html)
[php](https://www.php.net/downloads.php)

```console
mkdir /src && cd /src
curl -LO https://www.php.net/distributions/php-8.3.13.tar.gz
curl -LO https://curl.se/download/curl-8.11.0.tar.gz
mkdir -p curl && tar --strip-components 1 -xzf curl-*.tar.gz -C curl
mkdir -p php-src && tar --strip-components 1 -xzf php-*.tar.gz -C php-src
#git clone --recurse-submodules --depth 1 -b curl-8_11_0 https://github.com/curl/curl
#git clone --recurse-submodules --depth 1 -b php-8.3.13 https://github.com/php/php-src
git clone --recurse-submodules --depth 1 -b openssl-3.3.0-quic1 https://github.com/quictls/openssl
git clone --recurse-submodules --depth 1 -b v1.64.0 https://github.com/nghttp2/nghttp2
git clone --recurse-submodules --depth 1 -b v1.6.0 https://github.com/ngtcp2/nghttp3
git clone --recurse-submodules --depth 1 -b v1.8.1 https://github.com/ngtcp2/ngtcp2
```


## 2. Building for Debian 12
To build the packag, you can use the following Docker commands based on your shell.

```console
docker run -it --rm --name build1 -v src:/src -v ./AspNetCore.Nightmare:/Nightmare mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim bash /Nightmare/DependencyPackages/build.sh
```

Use "./" in Bash, "${PWD}/" in PowerShell, and "%CD%" in batch for current directory in command.


## 3. Test
After the build is completed, you can check the versions of required tools with the following commands.

```bash
+ openssl version
OpenSSL 3.3.0+quic 30 Jan 2024 (Library: OpenSSL 3.3.0+quic 30 Jan 2024)

+ curl --version
curl 8.11.0 (x86_64-pc-linux-gnu) libcurl/8.11.0 quictls/3.3.0 zlib/1.2.13 brotli/1.0.9 libpsl/0.21.2 nghttp2/1.64.0 ngtcp2/1.8.1 nghttp3/1.6.0
Release-Date: 2024-11-06
Protocols: dict file ftp ftps gopher gophers http https imap imaps ipfs ipns mqtt pop3 pop3s rtsp smb smbs smtp smtps telnet tftp ws wss
Features: alt-svc AsynchDNS brotli HSTS HTTP2 HTTP3 HTTPS-proxy IPv6 Largefile libz NTLM PSL SSL threadsafe TLS-SRP UnixSockets

+ curl --http3 'https://cloudflare-quic.com/cdn-cgi/trace'
fl=665f95
h=cloudflare-quic.com
ip=
ts=1731537264.406
visit_scheme=https
uag=curl/8.11.0
colo=ICN
sliver=none
http=http/3
loc=KR
tls=TLSv1.3
sni=plaintext
warp=on
gateway=off
rbi=off
kex=X25519

+ php --version
PHP 8.3.3 (cli) (built: Feb 29 2024 18:16:55) (ZTS)
Copyright (c) The PHP Group
Zend Engine v4.3.3, Copyright (c) Zend Technologies

+ php -c /Nightmare/Nightmare/config/php.ini -f /Nightmare/DependencyPackages/curltest.php
/build/curltest.php CURL_HTTP_VERSION_3: 30
fl=665f95
h=cloudflare-quic.com
ip=
ts=1731537264.406
visit_scheme=https
uag=curl/8.11.0
colo=ICN
sliver=none
http=http/3
loc=KR
tls=TLSv1.3
sni=plaintext
warp=on
gateway=off
rbi=off
kex=X25519
```
