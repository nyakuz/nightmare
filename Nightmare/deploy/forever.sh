#!/bin/sh
set +e

if [ ! -d "/app/forever" ]; then
	echo 'exit forever.sh'
	exit 1
fi

echo 'deploy forever.sh'

cd /app/forever

while true; do
	php -c /app/config/php.ini -r 'require "index.php";'
	sleep 30
done
