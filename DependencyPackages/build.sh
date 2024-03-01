export LD_LIBRARY_PATH=/usr/local/lib:/usr/local/lib64/

apt clean && apt update
apt install -y pkg-config build-essential autoconf bison re2c libtool dpkg-dev cmake
apt install -y zlib1g-dev libbrotli-dev libkrb5-dev libgmp-dev libonig-dev libsqlite3-dev libpq-dev libxml2-dev libargon2-dev libpsl-dev

export ARCH=$(dpkg-architecture -q DEB_BUILD_ARCH)
export ARCHDIR=$(uname -m)

cd /build && rm *.deb
mkdir /src
mkdir -p /usr/local-openssl/usr/local/ && mkdir -p /usr/deb-libssl-dev/usr/ && mkdir -p /usr/deb-libssl3/usr/lib && mkdir -p /usr/deb-libnghttp3/usr/local/ && mkdir -p /usr/deb-libngtcp2/usr/local/ && mkdir -p /usr/deb-libnghttp2/usr/local/ && mkdir -p /usr/deb-curl/usr/local/ && mkdir -p /usr/deb-php/usr/local/
set -e

cd /src/openssl/
export OPENSSL_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=openssl-)[\w\.]+' | head -1)
./config --api=3.0 --release -fPIC shared --prefix=/usr --openssldir=/usr/lib/ssl --libdir=/usr/lib/${ARCHDIR}-linux-gnu enable-tls1_3 shared no-idea no-md2 no-mdc2 no-zlib no-ssl3 enable-unit-test no-ssl3-method enable-rfc3779 enable-cms no-capieng no-rdrand
make clean && make -j$(nproc) && make DESTDIR=/usr/deb-openssl install -j$(nproc)
dpkg -r --force-depends openssl libssl3 libssl-dev
mv -fi /usr/deb-openssl/usr/lib/${ARCHDIR}-linux-gnu/ /usr/deb-libssl3/usr/lib/
mv -fi /usr/deb-openssl/usr/include/ /usr/deb-libssl-dev/usr/include/
mkdir -p /usr/deb-libssl3/DEBIAN/ && cp /build/deb/libssl3.txt /usr/deb-libssl3/DEBIAN/control
sed -i 's@$ARCHITECTURE@'${ARCH}'@' /usr/deb-libssl3/DEBIAN/control
sed -i 's@$VERSION@'${OPENSSL_VER}'@' /usr/deb-libssl3/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libssl3|cut -f 1)'@' /usr/deb-libssl3/DEBIAN/control
dpkg-deb -b /usr/deb-libssl3 /build/libssl3.deb
rm -R /usr/deb-libssl3/DEBIAN
mkdir -p /usr/deb-libssl-dev/DEBIAN/ && cp /build/deb/libssl-dev.txt /usr/deb-libssl-dev/DEBIAN/control
sed -i 's@$ARCHITECTURE@'${ARCH}'@' /usr/deb-libssl-dev/DEBIAN/control
sed -i 's@$VERSION@'${OPENSSL_VER}'@' /usr/deb-libssl-dev/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libssl-dev|cut -f 1)'@' /usr/deb-libssl-dev/DEBIAN/control
dpkg-deb -b /usr/deb-libssl-dev /build/libssl-dev.deb
rm -R /usr/deb-libssl-dev/DEBIAN
mkdir -p /usr/deb-openssl/DEBIAN/ && cp /build/deb/openssl.txt /usr/deb-openssl/DEBIAN/control
sed -i 's@$ARCHITECTURE@'${ARCH}'@' /usr/deb-openssl/DEBIAN/control
sed -i 's@$VERSION@'${OPENSSL_VER}'@' /usr/deb-openssl/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-openssl|cut -f 1)'@' /usr/deb-openssl/DEBIAN/control
dpkg-deb -b /usr/deb-openssl /build/openssl.deb
rm -R /usr/deb-openssl/DEBIAN
dpkg -i /build/libssl3.deb /build/libssl-dev.deb /build/openssl.deb

cd /src/nghttp3/
export NGHTTP3_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=v)[\w\.]+')
autoreconf -fi && ./configure --enable-lib-only
make clean && make install DESTDIR=/usr/deb-libnghttp3 -j$(nproc)
cp -ans /usr/deb-libnghttp3/usr/local/* /usr/local/
mkdir -p /usr/deb-libnghttp3/DEBIAN/ && cp /build/deb/libnghttp3.txt /usr/deb-libnghttp3/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-libnghttp3/DEBIAN/control
sed -i 's@$VERSION@'$NGHTTP3_VER'@' /usr/deb-libnghttp3/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libnghttp3|cut -f 1)'@' /usr/deb-libnghttp3/DEBIAN/control
dpkg-deb -b /usr/deb-libnghttp3 /build/libnghttp3.deb
rm -R /usr/deb-libnghttp3/DEBIAN

cd /src/ngtcp2/
export NGTCP2_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=v)[\w\.]+')
autoreconf -fi && ./configure --enable-lib-only
make clean && make install DESTDIR=/usr/deb-libngtcp2 -j$(nproc)
cp -ans /usr/deb-libngtcp2/usr/local/* /usr/local/
mkdir -p /usr/deb-libngtcp2/DEBIAN/ && cp /build/deb/libngtcp2.txt /usr/deb-libngtcp2/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-libngtcp2/DEBIAN/control
sed -i 's@$VERSION@'$NGTCP2_VER'@' /usr/deb-libngtcp2/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libngtcp2|cut -f 1)'@' /usr/deb-libngtcp2/DEBIAN/control
dpkg-deb -b /usr/deb-libngtcp2 /build/libngtcp2.deb
rm -R /usr/deb-libngtcp2/DEBIAN

cd /src/nghttp2/
export NGHTTP2_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=v)[\w\.]+')
autoreconf -fi && ./configure --enable-lib-only
make clean && make install DESTDIR=/usr/deb-libnghttp2 -j$(nproc)
cp -ans /usr/deb-libnghttp2/usr/local/* /usr/local/
mkdir -p /usr/deb-libnghttp2/DEBIAN/ && cp /build/deb/libnghttp2.txt /usr/deb-libnghttp2/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-libnghttp2/DEBIAN/control
sed -i 's@$VERSION@'$NGHTTP2_VER'@' /usr/deb-libnghttp2/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-libnghttp2|cut -f 1)'@' /usr/deb-libnghttp2/DEBIAN/control
dpkg-deb -b /usr/deb-libnghttp2 /build/libnghttp2.deb
rm -R /usr/deb-libnghttp2/DEBIAN

cd /src/curl-*/
if [[ ${PWD} == *"/src/curl"* ]]; then
	export CURL_VER=$(echo "${PWD}" | grep -oP '(?<=curl-)[\w\.]+')
else
	cd /src/curl/
	export CURL_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=curl-)[\w\.]+')
	export CURL_VER=${CURL_VER//_/.}
fi
autoreconf -fi && ./configure --disable-static --with-zlib --with-brotli --with-openssl --with-nghttp2 --with-ngtcp2 --with-nghttp3
make clean && make install DESTDIR=/usr/deb-curl -j$(nproc)
cp -ans /usr/deb-curl/usr/local/* /usr/local/
mkdir -p /usr/deb-curl/DEBIAN/ && cp /build/deb/curl.txt /usr/deb-curl/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-curl/DEBIAN/control
sed -i 's@$VERSION@'$CURL_VER'@' /usr/deb-curl/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-curl|cut -f 1)'@' /usr/deb-curl/DEBIAN/control
dpkg-deb -b /usr/deb-curl /build/curl.deb
rm -R /usr/deb-curl/DEBIAN

cd /src/php-src/
export PHP_VER=$(echo "$(git reflog show --decorate=full)" | grep -oP '(?<=php-)[\w\.]+' | head -1)
./buildconf --force && ./configure  'CFLAGS=-O2 -Wall -flto=auto -ffat-lto-objects -flto=auto -ffat-lto-objects -fstack-protector-strong -Wformat -Werror=format-security -fsigned-char -fno-strict-aliasing -fno-lto' --enable-opcache --enable-embed --enable-zts --enable-session --enable-bcmath --enable-mbstring --enable-phar --enable-ctype --enable-sysvmsg --enable-sysvsem --enable-sysvshm --enable-dom --enable-filter --enable-pdo --enable-soap --enable-sockets --enable-tokenizer --enable-mysqlnd --enable-mysqlnd-compression-support --with-gmp --with-password-argon2 --with-openssl=yes --with-iconv --with-mysqli --with-pdo-mysql=shared,mysqlnd --with-pdo-pgsql=shared,/usr --with-pdo-sqlite=shared,/usr --with-curl=shared,/usr --with-kerberos
make clean && make install INSTALL_ROOT=/usr/deb-php -j$(nproc)
cp -ans /usr/deb-php/usr/local/* /usr/local/
mkdir -p /usr/deb-php/DEBIAN/ && cp /build/deb/php.txt /usr/deb-php/DEBIAN/control
sed -i 's@$ARCHITECTURE@'$ARCH'@' /usr/deb-php/DEBIAN/control
sed -i 's@$VERSION@'$PHP_VER'@' /usr/deb-php/DEBIAN/control
sed -i 's@$SIZE@'$(du -s /usr/deb-php|cut -f 1)'@' /usr/deb-php/DEBIAN/control
dpkg-deb -b /usr/deb-php /build/php.deb
rm -R /usr/deb-php/DEBIAN


set -x
openssl version
curl --version
curl --http3 'https://cloudflare-quic.com/cdn-cgi/trace'
php --version
php -c /build/php.ini -f /build/curltest.php