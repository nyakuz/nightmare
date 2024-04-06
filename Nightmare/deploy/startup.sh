#!/bin/sh
echo 'startup.sh'
chmod 0777 /app/vhost
find /app/vhost -maxdepth 1 -type f -name "*.bak" -delete
find /app/ -maxdepth 1 -type f -name "*.sh" -exec chmod 0755 {} \;
find /app/ -maxdepth 1 -type f -name "*.json" -exec chown app:app {} \;
find /app/ -maxdepth 1 -type f -name "*.sh" -exec sed -i 's/\r/\n/g' {} \;
supervisorctl start app
