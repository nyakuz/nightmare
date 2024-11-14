# Build libxphp

### 1. php package
The package should be prepared in the path /Nightmare/DependencyPackages/build/*.deb.


### 2. Create Temporary Container for Build
You can use the following Docker commands based on your shell.

```console
docker run -it --rm --name build1 -v ./AspNetCore.Nightmare:/Nightmare -v src:/src mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim bash /Nightmare/libxphp/build.sh
```

Use "./" in Bash, "${PWD}/" in PowerShell, and "%CD%" in batch for current directory in command.


### 3. Set Up Build Environment

```console
apt clean && apt update
apt install -y pkg-config build-essential autoconf bison re2c libtool dpkg-dev cmake
pushd /Nightmare/DependencyPackages/build
apt install -y ./openssl.deb ./libssl3.deb ./libssl-dev.deb ./libnghttp3.deb ./libnghttp2.deb ./libngtcp2.deb ./curl.deb ./php.deb
popd
```

### 4. Build Thread Safe
To build the libxphp.so.

Release

```console
cd /Nightmare/libxphp
gcc -O2 -fPIC -shared -o libxphp.so main.cpp sapi.cpp ref.cpp sapi_module.cpp -DZTS -L/php/libxphp/ -L/usr/local/lib -I/usr/local/include/php -I/usr/local/include/php/main -I/usr/local/include/php/Zend -I/usr/local/include/php/TSRM -lphp -lstdc++
```

Debug

```console
cd /Nightmare/libxphp
gcc -g -fPIC -shared -o libxphp.so main.cpp sapi.cpp ref.cpp sapi_module.cpp -DZTS -L/php/libxphp/ -L/usr/local/lib -I/usr/local/include/php -I/usr/local/include/php/main -I/usr/local/include/php/Zend -I/usr/local/include/php/TSRM -lphp -lstdc++
```

### 5. libxphp.so
Move the libxphp.so from /Nightmare/libxphp/ to /Nightmare/Nightmare/Modules/.

```console
mv /Nightmare/libxphp/libxphp.so /Nightmare/Nightmare/Modules/
```
