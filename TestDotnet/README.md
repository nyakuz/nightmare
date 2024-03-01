# Debug libxphp

## 1. Create Temporary Container for Build
You can use the following Docker commands based on your shell.

PowerShell

	docker run -it --rm --name build1 -v ${PWD}/AspNetCore.Nightmare:/Nightmare -v src:/src mcr.microsoft.com/dotnet/sdk bash

bash

	docker run -it --rm --name build1 -v ${pwd}/AspNetCore.Nightmare:/Nightmare -v src:/src mcr.microsoft.com/dotnet/sdk bash


## 2. Set Up Build Environment

	apt clean && apt update
	apt install -y /Nightmare/openssl.deb /Nightmare/libssl3.deb /Nightmare/libssl-dev.deb /Nightmare/libnghttp3.deb /Nightmare/libnghttp2.deb /Nightmare/libngtcp2.deb /Nightmare/curl.deb /Nightmare/php.deb
	apt install -y libxml2 libpq5 libsqlite3-0 libonig5 libargon2-1
	cd /Nightmare/libxphp/testcpp


## 3. Build main.cpp

	gcc -g -o main main.cpp -DZTS -L/Nightmare/Nightmare/Modules -I/Nightmare/libxphp -I/usr/local/include/php -I/usr/local/include/php/main -I/usr/local/include/php/Zend -I/usr/local/include/php/TSRM -lxphp -lpthread -lstdc++


## 4. Debug

	export XPHP_CWD=/libxphp/testdotnet/
	export XPHP_INI=/libxphp/testdotnet/php.ini
	export LD_LIBRARY_PATH=/usr/local/lib:/libxphp
	export LD_PRELOAD=/usr/local/lib/libphp.so
	gdb ./main
	run