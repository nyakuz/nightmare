apt clean && apt update
apt install -y git

set -e
mkdir /src || true
cd /src && git clone --recurse-submodules $OPENSSL https://github.com/quictls/openssl
cd /src && git clone --recurse-submodules $NGHTTP3 https://github.com/ngtcp2/nghttp3
cd /src && git clone --recurse-submodules $NGTCP2 https://github.com/ngtcp2/ngtcp2
cd /src && git clone --recurse-submodules $NGHTTP2 https://github.com/nghttp2/nghttp2.git
cd /src && git clone --recurse-submodules $CURL https://github.com/curl/curl
cd /src && git clone --recurse-submodules $PHP https://github.com/php/php-src.git