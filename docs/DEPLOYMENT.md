# Deploying HelpDesk on a VPS

This guide puts HelpDesk on one Linux server with its own domain and HTTPS. It uses the same containers as the local setup
(`docker/README.md`) plus Caddy, a small web server that gets and renews the HTTPS certificate by itself.

```
Internet ──443──▶ caddy ──▶ web (API + Angular app, port 8080, not published)
                              └──▶ db (SQL Server, not published, data in a Docker volume)
```

Only Caddy listens on the server's ports 80 and 443. The app and the database cannot be reached from outside.

## 1. What you need

| | |
|---|---|
| Server | Linux x86_64 (amd64). **Not ARM**: the SQL Server image only exists for amd64. Ubuntu 24.04 LTS is used in the examples. |
| Size | 2 vCPU, 4 GB of RAM, 20 GB of disk. SQL Server refuses to run with less than 2 GB of RAM for itself. |
| Domain | A name such as `soporte.example.com` with an A record (and AAAA if you use IPv6) pointing to the server. Create it **before** the first start: Caddy needs it to get the certificate. |
| E-mail | An SMTP account that offers STARTTLS (port 587), for the e-mails the app sends (assigned, completed, approved, tracking link). |

## 2. Prepare the server

Run these as a normal user with `sudo`, not as root. Use SSH keys and turn password login off in `/etc/ssh/sshd_config`
(`PasswordAuthentication no`) once your key works.

```sh
# Firewall: SSH, HTTP (needed for the certificate) and HTTPS. Allow SSH first or you lock yourself out.
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 443/udp      # HTTP/3, optional
sudo ufw enable

# Automatic security updates of the system
sudo apt update && sudo apt install -y unattended-upgrades git
```

Install Docker Engine with Compose v2 from Docker's own repository, following
<https://docs.docker.com/engine/install/ubuntu/>. Then let your user run it, and check the version (**Compose 2.24 or newer** is needed):

```sh
sudo usermod -aG docker "$USER"     # log out and in again afterwards
docker compose version
```

Docker publishes container ports around `ufw`. That is why the production file does not publish the app or the database at all.

## 3. Get the code and set the secrets

```sh
sudo mkdir -p /opt/helpdesk && sudo chown "$USER": /opt/helpdesk
git clone https://github.com/emilioCode/HelpDesk.git /opt/helpdesk
cd /opt/helpdesk
git checkout main
cp .env.example .env
chmod 600 .env
```

Edit `.env`. These are the values to set on a server:

| Variable | What to put |
|---|---|
| `MSSQL_SA_PASSWORD` | A long random password: **letters, digits, `_` and `-` only**. Other symbols (`;` `=` `'` `$`) break the connection string the app builds from it. |
| `JWT_KEY` | A random value of at least 32 characters. Signs the sign-in tokens. Changing it signs everybody out. |
| `SECRETS_KEY` | A random value of at least 32 characters. Encrypts the companies' mail password in the database. **If you lose or change it, the saved mail passwords cannot be read again.** |
| `DOMAIN` | The public name, without `https://`: `soporte.example.com`. |
| `TZ` | The time zone of the business as an IANA name, for example `America/Santo_Domingo`. The app dates new orders with it. |
| `COMPOSE_FILE` | `docker-compose.yml:docker-compose.prod.yml`. With this line every `docker compose` command in this guide, and the backup script, uses the production setup. |

Generate a random value with `openssl rand -base64 48 | tr -d '+/=\n'`.

Keep a copy of `.env` somewhere safe outside the server (a password manager). You need `SECRETS_KEY` to restore a backup on another server.

## 4. First start, with the default password changed before anyone can reach it

A new database comes with one demo sign-in, account `root` with password `root` (`docker/db/02-seed.sql`). Do not let the server
answer on the internet with it. Start only the database first, change that password, and then start everything:

```sh
docker compose up -d db
docker compose up db-init                     # creates the database, prints "HelpDeskDB ready." and exits

read -rs -p "New password for root: " NEWPW; echo   # do not use the single quote character '
docker compose exec -T db sh -c '/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d HelpDeskDB -b -i /dev/stdin' <<SQL
UPDATE USUARIO SET contrasena = N'$NEWPW' WHERE cuenta_usuario = N'root';
SQL
unset NEWPW
```

The password is stored in clear text until `root` signs in for the first time, when the app replaces it with a hash. Sign in soon.

```sh
docker compose up -d --build                 # web and caddy start now
docker compose ps
docker compose logs caddy | grep -i -E "certificate|obtained"
```

The first build takes a few minutes. Open `https://<your domain>` and sign in as `root`. If the certificate is not issued, see section 9.

## 5. Set up the company

1. Sign in as `root` and open **Empresa**. Replace the demo company name and enter the mail server: host, port `587`, user, password and sender. The password is saved encrypted with `SECRETS_KEY`.
2. Open **Usuarios** and create the administrator and the technicians. Keep `root` for emergencies.
3. Create a test order and use **Enviar al cliente** with your own e-mail. The message must arrive with a link that starts with `https://<your domain>/t/` and the order must show the same link with **Copiar enlace**. The QR in the printed order uses the same link.

If an e-mail is not sent, the order is still saved and the reason is in `docker compose logs web` (wrong host, port, password).

## 6. The customer tracking link

Customers open the link without signing in. `docker-compose.prod.yml` sets `Tracking__PublicBaseUrl` to `https://<DOMAIN>`, so the e-mail
and the QR carry your public address. Two more settings can be added under `web` → `environment` in a compose override if you need them:

| Variable | Default | Meaning |
|---|---|---|
| `Tracking__DaysAfterApproval` | `30` | How long a link keeps working after the order is approved. |
| `Tracking__RequestsPerMinute` | `30` | Calls to the public page allowed per minute from one address. |

The limit is counted per visitor address. That only works because the app reads the real address that Caddy passes in `X-Forwarded-For`
(`ForwardedHeaders__Enabled=true`, already set by the production file). Without it, every customer would share one limit.

## 7. Backups

`scripts/backup-db.sh` writes a compressed full backup of the database to `/opt/helpdesk/backups`, checks it, and deletes copies older than 14 days
(`scripts/backup-db.sh 30` keeps 30).

```sh
cd /opt/helpdesk && ./scripts/backup-db.sh
```

Run it every night with `crontab -e`:

```
0 3 * * * cd /opt/helpdesk && ./scripts/backup-db.sh 14 >> /var/log/helpdesk-backup.log 2>&1
```

A backup on the same disk is not enough. Copy the `backups/` folder to another place every day (for example with `rclone` or `scp` to
another server or a storage bucket), and keep `.env` next to it.

### Restore

This replaces the current database with the backup. Take a fresh backup first if the database still works.

```sh
cd /opt/helpdesk
F=HelpDeskDB-YYYYMMDD-HHMMSS.bak               # the file in backups/
docker compose stop web
docker compose exec -T db mkdir -p /var/opt/mssql/backup
docker compose cp "backups/$F" "db:/var/opt/mssql/backup/$F"
docker compose exec -T -u root db chown mssql:root "/var/opt/mssql/backup/$F"      # without this SQL Server cannot read the file
docker compose exec -T db sh -c '/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -b -i /dev/stdin' <<SQL
ALTER DATABASE [HelpDeskDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [HelpDeskDB] FROM DISK = N'/var/opt/mssql/backup/$F' WITH REPLACE;
ALTER DATABASE [HelpDeskDB] SET MULTI_USER;
SQL
docker compose exec -T db rm -f "/var/opt/mssql/backup/$F"
docker compose start web
```

The same `SECRETS_KEY` must be in `.env`, or the saved mail passwords cannot be read. Try a restore once on a spare server before you need it.

## 8. Update to a new version

```sh
cd /opt/helpdesk
./scripts/backup-db.sh                  # always first
git pull
```

Read `docker/README.md`, section "Existing databases": when a version needs a change in the database, a script in `db/migrations/` must be run once.
Then rebuild and start:

```sh
docker compose up -d --build
docker compose ps
```

To go back, `git checkout` the previous version, rebuild, and restore the backup if a database script had already run.

### Day to day

```sh
docker compose ps                       # state of the containers
docker compose logs -f web              # the app's log (mail errors appear here)
docker compose restart web
docker image prune -f                   # frees the disk after several rebuilds
```

Set an outside check on `https://<your domain>/` with an uptime service so you hear about it before your users do.
Certificates renew by themselves; they live in the `caddy_data` volume.

## 9. When something is wrong

| Symptom | Likely cause |
|---|---|
| Caddy does not get a certificate | The domain does not point to this server yet, or ports 80 and 443 are closed. Check `docker compose logs caddy` and the firewall. |
| `required variable DOMAIN is missing` | `DOMAIN` is not set in `.env`. |
| The database container restarts or is unhealthy | Not enough memory (SQL Server needs at least 2 GB). See `docker compose logs db`. |
| Customers get "too many requests" after a few visits | The app does not see the real address. Make sure `ForwardedHeaders__Enabled` is `true` (set by the production file) and that the proxy sends `X-Forwarded-For`. |
| The tracking link starts with `http://` or a wrong name | `Tracking__PublicBaseUrl` is empty. Check `DOMAIN` and that `COMPOSE_FILE` includes `docker-compose.prod.yml` (`docker compose config | grep PublicBaseUrl`). |
| The mail never arrives | The company's mail data is wrong, or the server blocks outgoing port 587. See `docker compose logs web`. |
| After a restore, mail passwords are unreadable | `SECRETS_KEY` is not the one the backup was made with. Enter the mail password again in **Empresa**. |

## 10. Security checklist

- [ ] The password of `root` was changed before the first public start, and each person has their own account.
- [ ] Only ports 22, 80 and 443 are open (`sudo ufw status`), SSH uses keys, and `.env` is `chmod 600` and not in git.
- [ ] `AUTH_ENFORCE` is not set to `false`.
- [ ] `JWT_KEY`, `SECRETS_KEY` and the database password are random and are not the example values.
- [ ] Backups run every night and a copy leaves the server.

## Using your own proxy instead of Caddy

If the server already runs nginx or another proxy, do not use `docker-compose.prod.yml`. Run the base file with a small override that
publishes the app on the loopback address only, and let your proxy handle HTTPS:

```yaml
# docker-compose.override.yml
services:
  web:
    restart: unless-stopped
    ports: !override
      - "127.0.0.1:8080:8080"
```

In `.env` set `FORWARDED_HEADERS_ENABLED=true` and `TRACKING_PUBLIC_BASE_URL=https://soporte.example.com`. The app trusts the forwarded
address, so **it must not be reachable from anywhere but the proxy**: anybody who reaches port 8080 directly could write that header and avoid the limit.
The proxy must pass the WebSocket of the live updates (`/hub`) and these headers:

```nginx
location / {
    proxy_pass http://127.0.0.1:8080;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $remote_addr;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection $connection_upgrade;   # map $http_upgrade $connection_upgrade { default upgrade; '' close; }
}
```
