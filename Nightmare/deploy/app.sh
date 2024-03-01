#!/bin/sh

cd /app

while true; do
	dotnet Nightmare.dll
	sleep 1
done