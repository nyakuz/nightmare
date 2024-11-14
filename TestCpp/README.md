# TestCpp

## 1. Create Temporary Container for Build
You can use the following Docker commands based on your shell.

```console
docker run -it --rm --name debug1 --cap-add=NET_ADMIN --cap-add=SYS_PTRACE --security-opt seccomp=unconfined -v ./AspNetCore.Nightmare:/Nightmare -v src:/src mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim bash
```

Use "./" in Bash, "${PWD}/" in PowerShell, and "%CD%" in batch for current directory in command.


## 2. Set Up Build Environment

```console
apt clean && apt update
apt install -y pkg-config build-essential autoconf bison re2c libtool dpkg-dev cmake
pushd /Nightmare/DependencyPackages/build
apt install -y ./openssl.deb ./libssl3.deb ./libssl-dev.deb ./libnghttp3.deb ./libnghttp2.deb ./libngtcp2.deb ./curl.deb ./php.deb
popd
apt install -y libxml2 libpq5 libsqlite3-0 libonig5 libargon2-1
ln -s /usr/lib/$(uname -m)-linux-gnu/libssl.so /usr/lib/libssl.so.3
ln -s /usr/lib/$(uname -m)-linux-gnu/libcrypto.so /usr/lib/libcrypto.so.3
export LD_LIBRARY_PATH=/usr/local/lib:/Nightmare/Nightmare/Modules:$LD_LIBRARY_PATH
export XPHP_CWD=/Nightmare/TestCpp
export XPHP_INI=/Nightmare/TestCpp/php.ini
```


## 3. Build

```console
cd /Nightmare/TestCpp
gcc -g -o main main.cpp -DZTS -L/Nightmare/Nightmare/Modules -I/Nightmare/libxphp -I/usr/local/include/php -I/usr/local/include/php/main -I/usr/local/include/php/Zend -I/usr/local/include/php/TSRM -lphp -lxphp -lpthread -lstdc++
./main > output.html
```


## 4. Debug

```console
apt clean && apt update
apt install -y gdb
gdb ./main
run
```
