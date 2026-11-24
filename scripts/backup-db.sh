#!/bin/sh
# Full backup of HelpDeskDB into ./backups, keeping the last N days (14 by default).
#   scripts/backup-db.sh        or        scripts/backup-db.sh 30
# Run it from cron on the server (see docs/DEPLOYMENT.md). It needs the stack to be running.
set -eu
cd "$(dirname "$0")/.."

KEEP_DAYS="${1:-14}"
FILE="HelpDeskDB-$(date +%Y%m%d-%H%M%S).bak"
SQLCMD='/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -b'

mkdir -p backups
docker compose exec -T db mkdir -p /var/opt/mssql/backup
docker compose exec -T db sh -c "$SQLCMD -i /dev/stdin" <<SQL
BACKUP DATABASE [HelpDeskDB] TO DISK = N'/var/opt/mssql/backup/$FILE' WITH INIT, COMPRESSION, CHECKSUM;
RESTORE VERIFYONLY FROM DISK = N'/var/opt/mssql/backup/$FILE' WITH CHECKSUM;
SQL
docker compose cp "db:/var/opt/mssql/backup/$FILE" "backups/$FILE"
docker compose exec -T db rm -f "/var/opt/mssql/backup/$FILE"

find backups -name 'HelpDeskDB-*.bak' -type f -mtime "+$KEEP_DAYS" -delete
echo "Backup written to backups/$FILE"
