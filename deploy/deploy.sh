#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PUBLISH_DIR="$SCRIPT_DIR/../src/LaundryLog.UI/bin/Release/net10.0/publish/wwwroot"
VPS_USER="ivan"
VPS_HOST="vps.ivanthegeek.com"
VPS_PATH="/srv/runtipi/app-data/tipi-appstore/laundrylog/www/"
SSH_KEY="$HOME/.ssh/kvmtest_ssh_ivanthegeek_com"

dotnet publish "$SCRIPT_DIR/../src/LaundryLog.UI/LaundryLog.UI.fsproj" \
  --configuration Release

rsync -avz --delete \
  -e "ssh -i $SSH_KEY" \
  "$PUBLISH_DIR/" \
  "$VPS_USER@$VPS_HOST:$VPS_PATH"

echo "Deployed to https://laundrylog.ivanthegeek.com"
