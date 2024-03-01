# Build libxphp

## 1. php package
The package should be prepared in the path /Nightmare/php.deb.

## 2. Create Temporary Container for Build
You can use the following Docker commands based on your shell.

	docker run -it --rm --name build1 -v ./AspNetCore.Nightmare:/Nightmare -v src:/src mcr.microsoft.com/dotnet/sdk bash /Nightmare/libxphp/build.sh

In PowerShell, you should use ${pwd}/ instead of './'.

## 3. libxphp.so
Move the libxphp.so from /Nightmare/libxphp/ to /Nightmare/Nightmare/Modules/.

	mv /Nightmare/libxphp/libxphp.so /Nightmare/Nightmare/Modules/

## 3. Build Thread Safe (Release)
To build the libxphp.so.

	gcc -O2 -fPIC -shared -o libxphp.so main.cpp sapi.cpp ref.cpp -DZTS -L/php/libxphp/ -L/usr/local/lib -I/src/php-src -I/src/php-src/main -I/src/php-src/Zend -I/src/php-src/TSRM -lphp -lstdc++

## 4. Build Thread Safe (Debug)
To build the libxphp.so in debug mode.

	gcc -g -fPIC -shared -o libxphp.so main.cpp sapi.cpp ref.cpp -DZTS -L/php/libxphp/ -L/usr/local/lib -I/usr/local/include/php -I/usr/local/include/php/main -I/usr/local/include/php/Zend -I/usr/local/include/php/TSRM -lphp -lstdc++
