apt clean && apt update
apt install -y pkg-config build-essential autoconf bison re2c libtool dpkg-dev cmake
apt install -y zlib1g-dev libbrotli-dev libkrb5-dev libgmp-dev libonig-dev libsqlite3-dev libpq-dev libxml2-dev libargon2-dev
apt install -y /Nightmare/php.deb

cd /Nightmare/libxphp
gcc -O2 -fPIC -shared -o libxphp.so main.cpp sapi.cpp ref.cpp -DZTS -L/php/libxphp/ -L/usr/local/lib -I/usr/local/include/php -I/usr/local/include/php/main -I/usr/local/include/php/Zend -I/usr/local/include/php/TSRM -lphp -lstdc++