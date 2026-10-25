#!/usr/bin/env bash
# Checks a running stack: the start page loads, the API refuses anonymous calls when authentication is enforced,
# the seeded root user can sign in, the signed-in call works, and no response carries a password.
# Usage: scripts/ci-smoke.sh [base-url]   (run from the folder that has the .env used to start the stack)
set -euo pipefail

BASE=${1:-http://localhost:8080}
ENFORCE=$(sed -n 's/^AUTH_ENFORCE=//p' .env 2>/dev/null | head -1)
ENFORCE=${ENFORCE:-true}

fail() { echo "FAIL: $*"; exit 1; }
status() { curl -s -o /dev/null -w '%{http_code}' "$@"; }

echo "Waiting for the app at $BASE ..."
up=false
for _ in $(seq 1 60); do
  if curl -fs -o /dev/null "$BASE/"; then up=true; break; fi
  sleep 3
done
$up || fail "the app did not start in 3 minutes"

[ "$(status "$BASE/")" = 200 ] || fail "the start page does not load"
echo "ok  start page loads"

if [ "$ENFORCE" != false ]; then
  [ "$(status "$BASE/api/User/1/*")" = 401 ] || fail "an anonymous API call must answer 401"
  [ "$(status "$BASE/OrderReport?value=1-1")" = 401 ] || fail "a report without a session must answer 401"
  echo "ok  anonymous calls are refused"
else
  echo "--  AUTH_ENFORCE=false: anonymous calls are not checked"
fi

wrong=$(status -X POST "$BASE/api/Login" -H 'Content-Type: application/json' -d '{"CuentaUsuario":"root","Contrasena":"not-the-password"}')
[ "$wrong" = 204 ] || fail "a wrong password must answer 204, got $wrong"
echo "ok  a wrong password is refused"

login=$(curl -s -X POST "$BASE/api/Login" -H 'Content-Type: application/json' -d '{"CuentaUsuario":"root","Contrasena":"root"}')
token=$(echo "$login" | jq -r '.token // empty')
[ -n "$token" ] || fail "root cannot sign in"
echo "ok  root signs in and receives a token"

users=$(curl -s -H "Authorization: Bearer $token" "$BASE/api/User/1/*")
echo "$users" | jq -e 'type == "array" and length >= 1' > /dev/null || fail "the signed-in call returned no users"
echo "$users" | jq -e '[.[] | select(.contrasena != null)] | length == 0' > /dev/null || fail "the user list contains passwords"
echo "ok  the signed-in call works and returns no passwords"

echo "All checks passed."
