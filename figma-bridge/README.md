# Idle RPG Figma bridge

This is a local-only bridge for a Figma plugin. It is intentionally not a Figma REST-token integration.

## Safety defaults

- Listens only on `127.0.0.1:38471`.
- Generates a local token in `.bridge-token` on first start.
- `.bridge-token` is ignored and must never be committed.
- Requests need the token through `X-Bridge-Token` or `?token=`.
- Body size is capped at 64 KiB.
- Commands are limited to the allowlist in `bridge.py`.
- There is no delete command and no arbitrary JavaScript execution.

## Start

```powershell
cd C:\Users\Hoang PC\Documents\Idle-rpg\figma-bridge
python bridge.py serve
```

Keep this terminal open. The first run creates `.bridge-token`. In another terminal, print the token with:

```powershell
python bridge.py token
```

Paste that one-line value into the plugin UI only when instructed.

Health check from another terminal:

```powershell
curl http://127.0.0.1:38471/health
```

Send a test command after the plugin is connected:

```powershell
python bridge.py command create_frame --args '{"name":"AI Draft","width":1080,"height":2400,"x":0,"y":0}'
```

Run the local protocol self-check without starting a server:

```powershell
python test_bridge.py
```

## Install the plugin

1. Open Figma Desktop.
2. Go to **Plugins → Development → Import plugin from manifest...**.
3. Select `manifest.json` from this folder.
4. Run **Idle RPG UI Bridge** from **Plugins → Development**.
5. Enter the bridge URL and local token in the plugin window.
6. Click **Connect**.

The plugin code is intentionally limited to creating and styling draft nodes. It does not delete nodes or touch files outside the currently open Figma file.
