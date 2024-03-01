export OPENSSL="--depth 1 -b openssl-3.1.5+quic"
export NGHTTP3="--depth 1 -b v1.2.0"
export NGTCP2="--depth 1 -b v1.3.0"
export NGHTTP2="--depth 1 -b v1.59.0"
export CURL="--depth 1 -b curl-8_6_0"
export PHP="--depth 1 -b php-8.3.3"

`dirname $0`/download-latest.sh