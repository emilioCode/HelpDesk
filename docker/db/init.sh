#!/bin/sh
# Creates the HelpDeskDB schema and seed data once. Safe to run again: it does nothing if the database exists.
set -eu

SQLCMD="/opt/mssql-tools18/bin/sqlcmd -C -S db -U sa -P ${MSSQL_SA_PASSWORD}"

exists=$($SQLCMD -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'HelpDeskDB'" | tr -d '[:space:]')

if [ "$exists" = "0" ]; then
  echo "Creating HelpDeskDB..."
  $SQLCMD -b -i /db/01-schema.sql
  $SQLCMD -b -i /db/02-seed.sql
  echo "HelpDeskDB ready."
else
  echo "HelpDeskDB already exists, skipping."
fi
