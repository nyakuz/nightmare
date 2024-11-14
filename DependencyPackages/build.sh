#!/bin/sh
apt clean && apt update
apt install -y pkg-config build-essential autoconf bison re2c libtool dpkg-dev cmake git
apt install -y zlib1g-dev libbrotli-dev libkrb5-dev libgmp-dev libonig-dev libsqlite3-dev libpq-dev libxml2-dev libargon2-dev libpsl-dev

export LD_LIBRARY_PATH=/usr/local/lib:/usr/local/lib64/
export ARCH=$(dpkg-architecture -q DEB_BUILD_ARCH)
export ARCHDIR=$(uname -m)

mkdir -p /Nightmare/DependencyPackages/build && rm -f /Nightmare/DependencyPackages/build/*.deb && rm -R /usr/deb-*
mkdir /src
mkdir -p /usr/local-openssl/usr/local/ && mkdir -p /usr/deb-libssl-dev/usr/ && mkdir -p /usr/deb-libssl3/usr/lib && mkdir -p /usr/deb-libnghttp3/usr/local/ && mkdir -p /usr/deb-libngtcp2/usr/local/ && mkdir -p /usr/deb-libnghttp2/usr/local/ && mkdir -p /usr/deb-curl/usr/local/ && mkdir -p /usr/deb-php/usr/local/
set -e 


cd /src/openssl/
export OPENSSL_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=openssl-)[\w\.]+' | head -1)
./config --api=3.0 --release -fPIC shared --prefix=/usr --openssldir=/usr/lib/ssl --libdir=/usr/lib/${ARCHDIR}-linux-gnu enable-tls1_3 shared no-idea no-md2 no-mdc2 no-zlib no-ssl3 enable-unit-test no-ssl3-method enable-rfc3779 enable-cms no-capieng no-rdrand
make clean && make -j$(nproc) && make DESTDIR=/usr/deb-openssl install -j$(nproc)
dpkg -r --force-depends openssl libssl3 libssl-dev
mkdir -p /usr/deb-libssl3/usr/lib/${ARCHDIR}-linux-gnu/$(basename $(find /usr/deb-openssl/usr/lib/${ARCHDIR}-linux-gnu/ -type d -name engines-*))
mv -fi /usr/deb-openssl/usr/lib/${ARCHDIR}-linux-gnu/engines-*/* /usr/deb-libssl3/usr/lib/${ARCHDIR}-linux-gnu/$(basename $(find /usr/deb-openssl/usr/lib/${ARCHDIR}-linux-gnu/ -type d -name engines-*))
mv -fi /usr/deb-openssl/usr/lib/${ARCHDIR}-linux-gnu/ossl-modules /usr/deb-libssl3/usr/lib/${ARCHDIR}-linux-gnu/ossl-modules
mv -fi /usr/deb-openssl/usr/lib/${ARCHDIR}-linux-gnu/*.so.* /usr/deb-libssl3/usr/lib/${ARCHDIR}-linux-gnu/
mv -fi /usr/deb-openssl/usr/include /usr/deb-libssl-dev/usr/include
mkdir -p /usr/deb-libssl3/DEBIAN/ && cp /Nightmare/DependencyPackages/deb/libssl3.txt /usr/deb-libssl3/DEBIAN/control
sed -i 's@$ARCHITECTURE@'${ARCH}'@' /usr/deb-libssl3/DEBIAN/control
sed -i 's@$VERSION@'${OPENSSL_VER}'@' /usr/deb-libssl3/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libssl3|cut -f 1)'@' /usr/deb-libssl3/DEBIAN/control
dpkg-deb -b /usr/deb-libssl3 /Nightmare/DependencyPackages/build/libssl3.deb
rm -R /usr/deb-libssl3/DEBIAN
mkdir -p /usr/deb-libssl-dev/DEBIAN/ && cp /Nightmare/DependencyPackages/deb/libssl-dev.txt /usr/deb-libssl-dev/DEBIAN/control
sed -i 's@$ARCHITECTURE@'${ARCH}'@' /usr/deb-libssl-dev/DEBIAN/control
sed -i 's@$VERSION@'${OPENSSL_VER}'@' /usr/deb-libssl-dev/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libssl-dev|cut -f 1)'@' /usr/deb-libssl-dev/DEBIAN/control
dpkg-deb -b /usr/deb-libssl-dev /Nightmare/DependencyPackages/build/libssl-dev.deb
rm -R /usr/deb-libssl-dev/DEBIAN
mkdir -p /usr/deb-openssl/DEBIAN/ && cp /Nightmare/DependencyPackages/deb/openssl.txt /usr/deb-openssl/DEBIAN/control
sed -i 's@$ARCHITECTURE@'${ARCH}'@' /usr/deb-openssl/DEBIAN/control
sed -i 's@$VERSION@'${OPENSSL_VER}'@' /usr/deb-openssl/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-openssl|cut -f 1)'@' /usr/deb-openssl/DEBIAN/control
dpkg-deb -b /usr/deb-openssl /Nightmare/DependencyPackages/build/openssl.deb
rm -R /usr/deb-openssl/DEBIAN
cd /Nightmare/DependencyPackages/build && dpkg -i ./libssl3.deb ./libssl-dev.deb ./openssl.deb

cd /src/nghttp3/
export NGHTTP3_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=v)[\w\.]+')
autoreconf -fi && ./configure --enable-lib-only
make clean && make install DESTDIR=/usr/deb-libnghttp3 -j$(nproc)
cp -ans /usr/deb-libnghttp3/usr/local/* /usr/local/
mkdir -p /usr/deb-libnghttp3/DEBIAN/ && cp /Nightmare/DependencyPackages/deb/libnghttp3.txt /usr/deb-libnghttp3/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-libnghttp3/DEBIAN/control
sed -i 's@$VERSION@'$NGHTTP3_VER'@' /usr/deb-libnghttp3/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libnghttp3|cut -f 1)'@' /usr/deb-libnghttp3/DEBIAN/control
dpkg-deb -b /usr/deb-libnghttp3 /Nightmare/DependencyPackages/build/libnghttp3.deb
rm -R /usr/deb-libnghttp3/DEBIAN

cd /src/ngtcp2/
export NGTCP2_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=v)[\w\.]+')
autoreconf -fi && ./configure --enable-lib-only
make clean && make install DESTDIR=/usr/deb-libngtcp2 -j$(nproc)
cp -ans /usr/deb-libngtcp2/usr/local/* /usr/local/
mkdir -p /usr/deb-libngtcp2/DEBIAN/ && cp /Nightmare/DependencyPackages/deb/libngtcp2.txt /usr/deb-libngtcp2/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-libngtcp2/DEBIAN/control
sed -i 's@$VERSION@'$NGTCP2_VER'@' /usr/deb-libngtcp2/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libngtcp2|cut -f 1)'@' /usr/deb-libngtcp2/DEBIAN/control
dpkg-deb -b /usr/deb-libngtcp2 /Nightmare/DependencyPackages/build/libngtcp2.deb
rm -R /usr/deb-libngtcp2/DEBIAN

cd /src/nghttp2/
export NGHTTP2_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=v)[\w\.]+')
autoreconf -fi && ./configure --enable-lib-only
make clean && make install DESTDIR=/usr/deb-libnghttp2 -j$(nproc)
cp -ans /usr/deb-libnghttp2/usr/local/* /usr/local/
mkdir -p /usr/deb-libnghttp2/DEBIAN/ && cp /Nightmare/DependencyPackages/deb/libnghttp2.txt /usr/deb-libnghttp2/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-libnghttp2/DEBIAN/control
sed -i 's@$VERSION@'$NGHTTP2_VER'@' /usr/deb-libnghttp2/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libnghttp2|cut -f 1)'@' /usr/deb-libnghttp2/DEBIAN/control
dpkg-deb -b /usr/deb-libnghttp2 /Nightmare/DependencyPackages/build/libnghttp2.deb
rm -R /usr/deb-libnghttp2/DEBIAN

cd /src/curl/
if [ -d ".git" ]; then
	export CURL_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=curl-)[\w\.]+')
	export CURL_VER=${CURL_VER//_/.}
else
	export CURL_VER=$(basename $(ls -d ../curl-*.tar.gz | grep -oP '(?<=curl-)[\w\.]+' | head -1) .tar.gz)
fi
autoreconf -fi && ./configure --disable-static --with-zlib --with-brotli --with-openssl --with-nghttp2 --with-ngtcp2 --with-nghttp3
make clean && make install DESTDIR=/usr/deb-curl -j$(nproc)
dpkg -r --force-depends curl
cp -ans /usr/deb-curl/usr/local/* /usr/local/
mkdir -p /usr/deb-curl/DEBIAN/ && cp /Nightmare/DependencyPackages/deb/curl.txt /usr/deb-curl/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-curl/DEBIAN/control
sed -i 's@$VERSION@'$CURL_VER'@' /usr/deb-curl/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-curl|cut -f 1)'@' /usr/deb-curl/DEBIAN/control
dpkg-deb -b /usr/deb-curl /Nightmare/DependencyPackages/build/curl.deb
rm -R /usr/deb-curl/DEBIAN

cd /src/php-src/
if [ -d ".git" ]; then
	export PHP_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=php-)[\w\.]+' | head -1)
else
	export PHP_VER=$(basename $(ls -d ../php-*.tar.gz | grep -oP '(?<=php-)[\w\.]+' | head -1) .tar.gz)
fi
./buildconf --force && ./configure  'CFLAGS=-O2 -Wall -flto=auto -ffat-lto-objects -flto=auto -ffat-lto-objects -fstack-protector-strong -Wformat -Werror=format-security -fsigned-char -fno-strict-aliasing -fno-lto' --enable-opcache --enable-embed --enable-zts --enable-session --enable-bcmath --enable-mbstring --enable-phar --enable-ctype --enable-sysvmsg --enable-sysvsem --enable-sysvshm --enable-dom --enable-filter --enable-pdo --enable-soap --enable-sockets --enable-tokenizer --enable-mysqlnd --enable-mysqlnd-compression-support --with-gmp --with-password-argon2 --with-openssl=yes --with-iconv --with-mysqli --with-pdo-mysql=shared,mysqlnd --with-pdo-pgsql=shared,/usr --with-pdo-sqlite=shared,/usr --with-curl=shared,/usr --with-kerberos
make clean && make install INSTALL_ROOT=/usr/deb-php -j$(nproc)
cp -ans /usr/deb-php/usr/local/* /usr/local/
mkdir -p /usr/deb-php/DEBIAN/ && cp /Nightmare/DependencyPackages/deb/php.txt /usr/deb-php/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-php/DEBIAN/control
sed -i 's@$VERSION@'$PHP_VER'@' /usr/deb-php/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-php|cut -f 1)'@' /usr/deb-php/DEBIAN/control
dpkg-deb -b /usr/deb-php /Nightmare/DependencyPackages/build/php.deb
rm -R /usr/deb-php/DEBIAN

set -x
openssl version
curl --version
curl --http3 "https://cloudflare-quic.com/cdn-cgi/trace"
php --version
php -c /Nightmare/Nightmare/config/php.ini -f /Nightmare/DependencyPackages/curltest.php