# LaundryLog VPS Deployment Spec

**For the VPS session to implement. Fill in the bracketed placeholders from what it knows about the VPS.**

---

## What Needs to Exist on the VPS

### 1. Static file directory

Create a directory to hold the published app files:

```
/var/www/laundrylog/
```

Ownership should allow the deploy user (SSH user) to write to it via rsync.

### 2. Web server config — serve the static files

LaundryLog is a Blazor WebAssembly app — pure static files, no server runtime.
The web server must:

- Serve `/var/www/laundrylog/` at `[SUBDOMAIN_OR_PATH]` (e.g. `laundrylog.yourdomain.com` or `yourdomain.com/laundrylog`)
- Serve `.wasm` files with MIME type `application/wasm`
- Serve pre-compressed `.br` (Brotli) and `.gz` (gzip) files when the browser accepts them
- Return `index.html` for any path not found (SPA fallback — client-side routing)
- SSL via whatever cert management is already in place on the VPS

#### If nginx:

```nginx
server {
    listen 443 ssl;
    server_name [SUBDOMAIN];

    root /var/www/laundrylog;
    index index.html;

    # Brotli pre-compressed files
    brotli_static on;
    gzip_static on;

    # WASM MIME type
    types {
        application/wasm wasm;
    }

    # SPA fallback
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache _framework assets aggressively (fingerprinted filenames)
    location /_framework/ {
        add_header Cache-Control "public, max-age=31536000, immutable";
    }

    # SSL config managed by [CERT_MANAGER — certbot/acme/etc.]
}
```

#### If Caddy:

```
[SUBDOMAIN] {
    root * /var/www/laundrylog
    encode gzip zstd
    file_server
    try_files {path} /index.html
    header /_framework/* Cache-Control "public, max-age=31536000, immutable"
}
```

#### If runtipi is managing the reverse proxy (Traefik):

Need to know:
- Does runtipi support a "custom static site" app type, or should this be a standalone nginx container?
- What label/config format does Traefik expect for routing to a new backend?

---

## What Needs to Exist on This Dev Machine

Once the VPS session has set up the above, fill in this deploy script and commit it:

```
/home/ivan/nexus/LaundryLog/deploy/deploy.sh
```

Content (fill in placeholders):

```bash
#!/usr/bin/env bash
set -euo pipefail

PUBLISH_DIR="$(dirname "$0")/../src/LaundryLog.UI/bin/Release/net10.0/publish/wwwroot"
VPS_USER="[SSH_USER]"
VPS_HOST="[VPS_HOST_OR_IP]"
VPS_PATH="/var/www/laundrylog/"
SSH_KEY="[PATH_TO_SSH_KEY_IF_NOT_DEFAULT]"   # omit -i flag if using default ~/.ssh/id_*

dotnet publish "$(dirname "$0")/../src/LaundryLog.UI/LaundryLog.UI.fsproj" \
  --configuration Release

rsync -avz --delete \
  -e "ssh -i $SSH_KEY" \
  "$PUBLISH_DIR/" \
  "$VPS_USER@$VPS_HOST:$VPS_PATH"

echo "Deployed to https://[SUBDOMAIN]"
```

---

## Information Needed from the VPS Session

| Item | Notes |
|---|---|
| `[SSH_USER]` | User that can SSH in and write to `/var/www/` |
| `[VPS_HOST_OR_IP]` | Hostname or IP to rsync to |
| `[SSH_KEY]` | Path to private key on this machine, if non-default |
| `[SUBDOMAIN]` | Where the app will be reachable (e.g. `laundrylog.irxops.com`) |
| Web server type | nginx / Caddy / Traefik-via-runtipi |
| SSL / cert management | certbot, acme.sh, Caddy auto, Traefik ACME |
| runtipi proxy approach | Does runtipi manage all inbound traffic, or is there a standalone nginx alongside it? |

---

## Published Output Facts (for VPS session reference)

- Build command: `dotnet publish src/LaundryLog.UI/LaundryLog.UI.fsproj --configuration Release`
- Output directory: `src/LaundryLog.UI/bin/Release/net10.0/publish/wwwroot/`
- Size: ~33 MB (includes .NET WASM runtime — large on first load, cached after)
- Pre-compressed: `.br` and `.gz` variants are generated automatically for most assets
- No server runtime required — purely static files
