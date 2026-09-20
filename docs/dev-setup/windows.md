# Developer Setup: Windows 11

Nibras servers are Linux, but Windows is a first-class development machine. Everything below is tested by the `dev-smoke` pipeline job on a Windows runner, so these instructions are verified rather than remembered.

**Shell.** Use **PowerShell 7** or **Git Bash**. Windows PowerShell 5.1 works for most things but lacks pipeline chain operators, so the guides assume PowerShell 7.

---

## 1. Install the toolchain

```powershell
winget install --id Microsoft.PowerShell -e
winget install --id Git.Git -e
winget install --id OpenJS.NodeJS.LTS -e
winget install --id Microsoft.DotNet.SDK.10 -e
winget install --id RedHat.Podman-Desktop -e
```

Flutter and Android Studio are needed only for mobile work:

```powershell
winget install --id Google.AndroidStudio -e
winget install --id Google.Flutter -e
```

Then the Aspire workload:

```powershell
dotnet workload install aspire
```

## 2. Configure Git for a mixed-operating-system repository

```powershell
git config --global core.autocrlf false
git config --global core.longpaths true
```

`core.autocrlf false` is deliberate. The repository normalises line endings through `.gitattributes`, and letting Git convert them as well produces files that differ between machines. `core.longpaths` prevents a checkout failing on a deep path; the lint also keeps every path under 200 characters so this should never be needed, and it costs nothing to set.

## 3. Choose a container runtime

Docker Desktop is **not required**, and the licence policy in master brief Section 6.2 means it should not be assumed. Two supported options:

**Podman Desktop.** Start the machine, then point Testcontainers at it:

```powershell
podman machine init --cpus 4 --memory 8192 --disk-size 60
podman machine start
$env:DOCKER_HOST = "npipe:////./pipe/podman-machine-default"
$env:TESTCONTAINERS_RYUK_DISABLED = "true"
```

Ryuk is disabled because its container cannot always reach the Podman socket to clean up. Containers are then removed by the test fixtures instead.

**Docker Engine inside WSL2.** Install Ubuntu from the Microsoft Store, install Docker Engine inside it, and expose the socket over TCP to the Windows host. This is the closer match to the production runtime and the better choice if you also want to run the compose stack.

Set these permanently rather than per session:

```powershell
[Environment]::SetEnvironmentVariable("DOCKER_HOST", "npipe:////./pipe/podman-machine-default", "User")
```

## 4. Exclude the build directories from Windows Defender

Real-time scanning of `node_modules`, `bin` and `obj` costs minutes on every build.

```powershell
Add-MpPreference -ExclusionPath "$PWD\src"
Add-MpPreference -ExclusionProcess "dotnet.exe"
Add-MpPreference -ExclusionProcess "node.exe"
```

Run that in an elevated shell, and point it at the repository you actually cloned.

## 5. Verify

```powershell
tools\dev-setup\verify-setup.ps1
```

It checks the versions, the container runtime, the Git configuration, and that the kit lint runs. It prints one line per check and exits non-zero on the first real problem. `/verify-setup` runs the same thing and explains any failure.

---

## Known Windows issues

| Symptom | Cause | Fix |
|---|---|---|
| Testcontainers hangs on start | Podman machine not running, or `DOCKER_HOST` unset | `podman machine start`, then set `DOCKER_HOST` as above |
| Ryuk container fails | Ryuk cannot reach the Podman socket | `TESTCONTAINERS_RYUK_DISABLED=true` |
| Android emulator will not start | Hyper-V and the emulator hypervisor conflict | Use the Windows Hypervisor Platform backend, or run the emulator with Hyper-V disabled for that session |
| A checkout fails with a path error | A path over the Windows limit | `core.longpaths true`; report it, because the lint should have caught it |
| A file appears modified with no change | Line ending conversion | `core.autocrlf false`, then `git checkout -- .` |
| A test passes locally and fails on the Linux runner | Culture-sensitive parsing, or a case-only path difference | Pin the culture explicitly; run `/audit-portability` |
| `dotnet test` is slow on the first run | Defender scanning the build output | Add the exclusions above |

## What you cannot do on Windows

| Task | Why | What to do |
|---|---|---|
| Build the iOS application | Apple tooling requires macOS | The macOS pipeline job builds it. See Open Question 14 |
| Run the production images natively | All images are Linux | Use the container runtime, which is what production uses |
| Host an on-premises installation on Windows Server | Several dependencies have no Windows server build | Use the Linux virtual machine appliance. ADR-0016 |
