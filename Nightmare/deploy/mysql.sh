#!/bin/sh
result = $(mysqld --version)
if [[ $result = *"command not found" ]]; then
	exit 1
if

while true; do
	mysqld --defaults-file=/app/config/my.conf --user=root
	sleep 1
done