# Build Package

## 1. 
Curl recommends downloading the official release of the source file.

[curl](https://curl.se/download.html)

    mkdir /src
    tar -xzf /build/curl-8.6.0.tar.gz -C /src

## 2. Building for Debian 12
To build the packag, you can use the following Docker commands based on your shell.

    docker run -it --rm --name build1 -v ./AspNetCore.Nightmare/DependencyPackages:/build mcr.microsoft.com/dotnet/aspnet:latest bash
    /build/download-stable.sh
    /build/build.sh

In PowerShell, you should use ${pwd}/ instead of './'.

## 3. Test
After the build is completed, you can check the versions of required tools with the following commands.

```bash
++ openssl version
OpenSSL 3.1.5+quic 30 Jan 2024 (Library: OpenSSL 3.1.5+quic 30 Jan 2024)

curl 8.3.0-DEV (x86_64-pc-linux-gnu) libcurl/8.3.0-DEV OpenSSL/3.0.10 zlib/1.2.13 brotli/1.0.9 nghttp2/1.56.0 ngtcp2/0.19.1 nghttp3/0.15.0

++ curl --version
curl 8.6.0 (aarch64-unknown-linux-gnu) libcurl/8.6.0 quictls/3.1.5 zlib/1.2.13 brotli/1.0.9 libpsl/0.21.2 nghttp2/1.59.0 ngtcp2/1.3.0 nghttp3/1.2.0
Release-Date: 2024-01-31
Protocols: dict file ftp ftps gopher gophers http https imap imaps ipfs ipns mqtt pop3 pop3s rtsp smb smbs smtp smtps telnet tftp
Features: alt-svc AsynchDNS brotli HSTS HTTP2 HTTP3 HTTPS-proxy IPv6 Largefile libz NTLM PSL SSL threadsafe TLS-SRP UnixSockets

+ curl --http3 'https://cloudflare-quic.com/cdn-cgi/trace'
fl=34f228
h=cloudflare-quic.com
ts=1695201398.246
visit_scheme=https
uag=curl/8.3.0-DEV
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

++ php --version
PHP 8.3.3 (cli) (built: Feb 29 2024 18:16:55) (ZTS)
Copyright (c) The PHP Group
Zend Engine v4.3.3, Copyright (c) Zend Technologies

+ php -c /build/php.ini -f /build/curltest.php
/build/curltest.php CURL_HTTP_VERSION_3: 30
fl=34f228
h=cloudflare-quic.com
ts=1695201913.947
visit_scheme=https
uag=
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

## Package List
| Name | Branch | Version | Result Path |
| --- | --- | --- | --- |
| OpenSSL+quictls | [openssl-3.0.10+quic](https://github.com/quictls/openssl/tree/openssl-3.0.10) | 3.0.10+quic | /build/openssl.deb |
| libssl3 | --- | --- | /build/libssl3.deb |
| libssl-dev | --- | --- | /build/libssl-dev.deb |
| NgHttp2 | [v1.56.0](https://github.com/nghttp2/nghttp2/tree/v1.56.0) | 1.56.0 | /build/libnghttp2.deb |
| NgHttp3 | [v0.15.0](https://github.com/ngtcp2/nghttp3/tree/v0.15.0) | 0.15.0 | /build/libnghttp3.deb |
| NgTcp2 | [v0.19.1](https://github.com/ngtcp2/ngtcp2/tree/v0.19.1) | 0.19.1 | /build/libngtcp2.deb |
| Curl | [curl-8_3_0](https://github.com/curl/curl/tree/curl-8_3_0) | 8.3.0 | /build/curl.deb |
| Php | [php-8.2.10](https://github.com/php/php-src/tree/php-8.2.10) | 8.2.10 | /build/php.deb |
