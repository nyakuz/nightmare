export OPENSSL="--depth 1 -b openssl-3.0.10+quic"
export NGHTTP2="--depth 1 -b v1.56.0"
export NGTCP2="--depth 1 -b v0.19.1"
export NGHTTP3="--depth 1 -b v0.15.0"
export CURL="--depth 1 -b curl-8_3_0"
export PHP="--depth 1 -b php-8.2.10"

`dirname $0`/download-latest.sh