#!/bin/sh
set +e

result = $(mysqld --version)
if [[ $result = *"not found" ]]; then
	echo 'exit mysql'
	exit 1
if

echo 'deploy mysql'

while true; do
	mysqld --defaults-file=/app/config/my.conf --user=root
	sleep 1
done
