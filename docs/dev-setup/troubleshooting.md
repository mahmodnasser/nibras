# Troubleshooting

Problems that cost a day if you have not seen them before, grouped by what you were trying to do. Run `tools/dev-setup/verify-setup.ps1` or `.sh` first: it catches most of these before you hit them.

---

## Containers and integration tests

| Symptom | Cause | Fix |
|---|---|---|
| Testcontainers hangs on start, no error | No container runtime reachable | Start Podman or Docker, then set `DOCKER_HOST`. On Windows, `npipe:////./pipe/podman-machine-default` |
| Ryuk container fails to start or times out | Ryuk cannot reach the Podman socket to clean up | `TESTCONTAINERS_RYUK_DISABLED=true`. The fixtures remove their own containers |
| Tests pass alone, fail together | Shared state between fixtures, or a port collision | Each fixture gets its own container and a random port. Never a fixed port |
| PostgreSQL container starts, tests cannot connect | The host cannot reach the container network, common with Podman on Windows | Use the mapped port the fixture reports, never a hard-coded one |
| A test that reads a pooled connection sees another tenant's rows | `SET` used instead of `SET LOCAL` with transaction pooling | `SET LOCAL` inside the transaction. Reference architecture Section 14. There is a test for exactly this |
| Containers accumulate after a crashed run | Ryuk disabled and the process died | `docker ps -aq | xargs docker rm -f`, or the Podman equivalent |

## Builds

| Symptom | Cause | Fix |
|---|---|---|
| A first build takes minutes on Windows | Defender scanning `node_modules`, `bin`, `obj` | Add the exclusions in `windows.md` |
| Angular or Flutter build fails with too many open files | inotify watch limit on Linux | Raise `fs.inotify.max_user_watches` |
| A file shows as modified with no change | Line ending conversion | `core.autocrlf false`, then `git checkout -- .` |
| A checkout fails with a path error on Windows | A path over the limit | `core.longpaths true`. Also report it: lint rule R14 should have prevented it |
| Two files became one after a pull on macOS | Case-insensitive file system | Lint rule R13 prevents this being committed; rename on Linux to recover |

## Tests that differ between machines

| Symptom | Cause | Fix |
|---|---|---|
| A money or date test passes on Windows and fails on Linux | The machine culture leaked into a parse or format | Pin the culture explicitly. Invariant for anything stored or transmitted |
| A Hijri date is one day out after an update | The ICU version changed | ICU is pinned per release. If it moved, that is the bug |
| A bell schedule shifts for one region | tzdata updated | tzdata is pinned per release |
| Arabic renders as boxes in a generated document | Fonts missing from the image | They are bundled in the Documents and Gotenberg images. Rendering outside them needs them installed |
| A Flutter golden differs locally from the pipeline | Font rendering differs by operating system | Goldens are generated on the Linux runner and are authoritative. Do not regenerate locally |
| A test passes locally and fails in the pipeline with a missing file | Case-only difference in an import | Lint rule R13, and `/audit-portability` |

## Running the stack

| Symptom | Cause | Fix |
|---|---|---|
| `aspire run` starts but a service is unhealthy | A dependency container is still starting | Readiness probes gate it. Check the dashboard, not the log tail |
| RabbitMQ management shows queues but no consumers | The worker host did not start, or its profile is off | Workers are separate images; check the compose profile |
| Redis is down and everything stops | The circuit breaker is not wired for that path | It must degrade to L1 and the database. Master brief Section 19 requires it, and there is a resilience test |
| The `ai` profile fails to start | No model server, or no capable hardware | It is off by default and the product is complete without it. Section 25 |
| Mail never arrives in development | Mailpit is the development transport | Open the Mailpit interface; nothing leaves the machine |

## Packaging the kit

| Symptom | Cause | Fix |
|---|---|---|
| The archive extracts on Windows but fails on Linux with a backslash warning | PowerShell `Compress-Archive` writes backslash path separators, which the zip format does not define as a separator | Build the archive with `tar -a -c -f out.zip *` (bsdtar ships with Windows and Git Bash). It writes forward slashes and extracts everywhere |
| Extracted file count differs from the source | A hidden directory was missed by the glob | Archive from inside the kit root with `*`, and compare `find . -type f | wc -l` on both sides |

## The kit itself

| Symptom | Cause | Fix |
|---|---|---|
| `kit-lint` reports a missing appendix | A document references an appendix that does not exist | Create it or fix the reference. `/lint-plan` names the minimal fix |
| `kit-lint` reports version drift | One brief file was edited without the others | All three carry the same version. This needs an ADR too |
| A hook prints errors after every edit | The kit has real consistency errors | Fix them. The hook never blocks, so they accumulate silently otherwise |
| A hook does nothing on Windows | It was written for bash | Every hook invokes node with a relative path. Lint rule R16 |
| `kit-lint` fails on a file you did not touch | Another parallel change landed | Run it again after pulling; the rules are order-independent |
