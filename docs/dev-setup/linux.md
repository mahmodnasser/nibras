# Developer Setup: Linux

Ubuntu 22.04 or later, or the Red Hat family. This is the closest match to production, and the `dev-smoke` pipeline job runs these steps on a Linux runner.

---

## 1. Install the toolchain

```bash
# .NET 10 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Node 22 LTS
curl -fsSL https://deb.nodesource.com/setup_22.x | sudo -E bash -
sudo apt-get install -y nodejs

# Container runtime: Docker Engine, or Podman
sudo apt-get install -y docker.io
sudo usermod -aG docker "$USER"   # log out and back in for this to take effect

# Aspire
dotnet workload install aspire
```

Flutter is needed only for mobile work:

```bash
sudo snap install flutter --classic
flutter doctor
```

## 2. Configure Git

```bash
git config --global core.autocrlf false
```

The repository normalises line endings through `.gitattributes`. Leave Git out of it.

## 3. Raise the file watcher limit

Angular and Flutter both watch large trees, and the default limit is low enough to cause confusing build failures.

```bash
echo "fs.inotify.max_user_watches=524288" | sudo tee -a /etc/sysctl.conf
sudo sysctl -p
```

## 4. Verify

```bash
tools/dev-setup/verify-setup.sh
```

It checks versions, the container runtime, the Git configuration and the kit lint, printing one line per check. `/verify-setup` runs the same thing and explains any failure.

---

## Known Linux issues

| Symptom | Cause | Fix |
|---|---|---|
| Permission denied talking to the container socket | The user is not in the docker group yet | Log out and back in after `usermod` |
| Testcontainers cannot reach the database | Podman socket not enabled | `systemctl --user enable --now podman.socket`, then set `DOCKER_HOST` to that socket |
| Builds fail with too many open files | inotify watch limit | Raise it as above |
| Arabic renders as boxes in a generated PDF | The fonts are missing from the container | They are bundled in the image; if you are rendering outside it, install Inter and IBM Plex Sans Arabic |
| A date or number differs from a colleague's Windows run | The machine culture leaked into a parse or format | Pin the culture; run `/audit-portability` |

## Running the whole stack

```bash
# Everything, through Aspire, with a dashboard
cd src/AppHost && aspire run

# Or through compose
docker compose -f deploy/compose/docker-compose.yml --profile dev up
```

The `dev` profile brings up PostgreSQL, PgBouncer, RabbitMQ, both Redis roles, SeaweedFS, Gotenberg, Mailpit and ClamAV, plus the services. The `ai` profile adds a local model server and is off by default, because master brief Section 25 requires the product to be complete without it.
