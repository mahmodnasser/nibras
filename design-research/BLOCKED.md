# Design research: blocked by network policy

Date: 2026-09-26

The design research could not start. This environment's outbound network policy blocks every reference host. None of the demo sites was visited, and no screenshots or report were produced.

## What was run

```bash
curl -sS -o /dev/null -w "%{http_code}" <url>
```

Result for each URL: `curl: (56) CONNECT tunnel failed, response 403`. The agent proxy recorded each one as `connect_rejected: gateway answered 403 to CONNECT (policy denial or upstream failure)`.

| Host | URL tested | Result |
|---|---|---|
| `wowtheme7.com` | https://wowtheme7.com/tf/edudash/demo/index.html | 403 at CONNECT |
| `edumin.dexignlab.com` | https://edumin.dexignlab.com/xhtml/event-management.html | 403 at CONNECT |
| `akademi-vite.vercel.app` | https://akademi-vite.vercel.app/dashboard | 403 at CONNECT |
| `elements.envato.com` | https://elements.envato.com/edudash-laravel-school-college-lms-admin-K3JFC4Q | 403 at CONNECT |

Playwright/Chromium would use the same proxy, so it would be blocked too. It was not tried separately.

## How to unblock

Open the cloud environment's settings (environment menu in the session title bar, then Edit), then change **Network access**. Either choose a broader access level, or add these hosts to the allowed domains:

- `wowtheme7.com`
- `edumin.dexignlab.com`
- `akademi-vite.vercel.app`
- `elements.envato.com`

The pages may also load assets from CDNs (fonts, scripts, images), and those hosts would need to be allowed too. Access levels are described at https://code.claude.com/docs/en/claude-code-on-the-web.

Once the hosts are allowed, run the research task again in a new session.
