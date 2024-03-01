#!/bin/sh
if [ ! -d "/app/forever" ]; then
	exit
fi

cd /app/forever

while true; do
	php -c /app/config/php.ini -r 'require "index.php";'
	sleep 30
done