# Developer Setup: macOS

macOS 14 or later, Apple silicon or Intel. macOS is supported for development and is **required for building the iOS application**, which is the one task no other operating system can do.

---

## 1. Install the toolchain

```bash
brew install --cask dotnet-sdk
brew install node
brew install podman        # or: brew install --cask docker, licence permitting
brew install --cask flutter
dotnet workload install aspire
```

For iOS work:

```bash
xcode-select --install
sudo xcodebuild -license accept
```

Xcode itself comes from the App Store and is needed in full, not just the command line tools, to produce an archive.

## 2. Configure Git

```bash
git config --global core.autocrlf false
```

**macOS file systems are case-insensitive by default.** Two files differing only by case will silently collapse into one here and then break on the Linux runner. The kit lint catches this, and it is worth knowing why the rule exists.

## 3. Start the container runtime

```bash
podman machine init --cpus 4 --memory 8192 --disk-size 60
podman machine start
export DOCKER_HOST="unix://$(podman machine inspect --format '{{.ConnectionInfo.PodmanSocket.Path}}')"
export TESTCONTAINERS_RYUK_DISABLED=true
```

## 4. Verify

```bash
tools/dev-setup/verify-setup.sh
```

---

## Building for iOS

This is the reason a Mac is on the list at all.

```bash
cd src/Mobile
flutter build ipa --flavor <flavor> --export-options-plist=ios/ExportOptions.plist
```

**Signing.** The shared multi-school application is published by the platform. A white-label application is published under the school's own developer account, with the platform building and submitting on its behalf under a written agreement; the school owns the certificates and the push credentials. Master brief Section 37 has the full position, and it is worth settling before the first white-label sale rather than after.

**In the pipeline.** `ci-mobile-ios.yml` runs on a hosted macOS runner and is path-filtered to `src/Mobile/**`, so it does not run on every commit. Its cost is a line in master brief Section 30, and whether we buy runner minutes or a machine is Open Question 14.

---

## Known macOS issues

| Symptom | Cause | Fix |
|---|---|---|
| Two files collapse into one after a pull | The file system is case-insensitive | The lint rule R13 prevents this being committed; if it happens, rename on a Linux machine |
| Testcontainers cannot find the socket | `DOCKER_HOST` not exported for the Podman machine | Export it as above |
| A Flutter golden test differs from the pipeline | Font rendering differs between macOS and the Linux runner | Goldens are generated on the Linux runner and are authoritative; do not regenerate them locally |
| Xcode refuses to archive | Signing identity or provisioning profile missing | Check which developer account the flavour belongs to before assuming a tooling problem |
