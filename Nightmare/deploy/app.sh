#!/bin/sh
set +e
echo 'deploy app.sh'

cd /app

while true; do
	dotnet Nightmare.dll
	sleep 1
done
