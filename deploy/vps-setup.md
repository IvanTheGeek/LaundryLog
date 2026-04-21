# LaundryLog VPS Setup

---

## Context

LaundryLog is a Blazor WebAssembly app (pure static files). It is served by an nginx
container (`fholzer/nginx-brotli`) managed by RunTIPI on the VPS.

Traefik (RunTIPI's reverse proxy) handles SSL and routes `laundrylog.ivanthegeek.com`
to the nginx container. The nginx config and static files are bind-mounted from the host
into the container so that rsync from the dev machine updates the live site.

**VPS:** OVH `vps-482d130f.vps.ovh.us` / `15.204.119.184` / `2604:2dc0:202:300::268b`
**SSH:** `ssh -i /home/ivan/kvmtest_ssh_ivanthegeek_com ivan@15.204.119.184`

DNS:
- `laundrylog.ivanthegeek.com` A → `15.204.119.184`
- `laundrylog.ivanthegeek.com` AAAA → `2604:2dc0:202:300::268b`

RunTIPI install path: `/srv/runtipi/` (installed as debian user; symlinked at `/home/ivan/runtipi`)

App data path: `/srv/runtipi/app-data/tipi-appstore/laundrylog/`
- `www/` — static files; rsync target from dev machine
- `conf/default.conf` — nginx site config

---

## Setup Status (OVH VPS)

| Item | Status | Notes |
|---|---|---|
| VPS provisioned | Done | Debian 13, Docker 29.4.1 |
| SSH hardened | Done | PermitRootLogin no, PasswordAuthentication no |
| Swap | Done | 2 GiB, swappiness=10 |
| ufw | Done | allow 22/80/443 |
| ivan user | Done | sudo (NOPASSWD), docker groups; debian locked |
| RunTIPI v4.8.2 | Done | 4 containers healthy |
| RunTIPI domain set | Done | runtipi.ivanthegeek.com |
| RunTIPI admin secured | Done | Password changed + 2FA enabled |
| Custom appstore added | Done | https://github.com/IvanTheGeek/tipi-appstore |
| Directories created | Done | www/ and conf/ under app-data |
| nginx config written | Done | No types{} block needed; wasm already in mime.types |
| LaundryLog installed | Done | Container running |
| HTTPS verified | Done | Let's Encrypt cert issued; HTTP/2 200 |
| Deployed | Done | Blazor app live at https://laundrylog.ivanthegeek.com |

---

## nginx config (at `conf/default.conf`)

```nginx
server {
    listen 80;
    server_name laundrylog.ivanthegeek.com;

    root /usr/share/nginx/html;
    index index.html;

    brotli_static on;
    gzip_static on;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /_framework/ {
        add_header Cache-Control "public, max-age=31536000, immutable";
    }
}
```

Note: no `types {}` block needed — `application/wasm` is already in the image's
`/etc/nginx/mime.types`.

---

## Verify

```bash
curl -I https://laundrylog.ivanthegeek.com
```

Expected: `HTTP/2 200`, `content-type: text/html`

---

## Deploy

```bash
cd /home/ivan/nexus/LaundryLog
./deploy/deploy.sh
```

Publishes the Blazor app and rsyncs to
`ivan@15.204.119.184:/srv/runtipi/app-data/tipi-appstore/laundrylog/www/`
