# Debug libxphp

## 1. Create Temporary Container for Build
You can use the following Docker commands based on your shell.

PowerShell

	docker run -it --rm --name build1 -v ${PWD}/AspNetCore.Nightmare:/Nightmare -v src:/src mcr.microsoft.com/dotnet/sdk bash

bash

	docker run -it --rm --name build1 -v ${pwd}/AspNetCore.Nightmare:/Nightmare -v src:/src mcr.microsoft.com/dotnet/sdk bash


## 2. Set Up Build Environment

	apt clean && apt update
	apt install -y gdb
	cd /Nightmare/libxphp/testcpp


## 3. Build main.cpp

	gcc -g -o main main.cpp -DZTS -L/Nightmare/Nightmare/Modules -I/Nightmare/libxphp -I/usr/local/include/php -I/usr/local/include/php/main -I/usr/local/include/php/Zend -I/usr/local/include/php/TSRM -lxphp -lpthread -lstdc++


## 4. Debug

	export XPHP_CWD=/Nightmare/libxphp/testcpp/
	export XPHP_INI=/Nightmare/libxphp/testcpp/php.ini
	gdb ./main
	run