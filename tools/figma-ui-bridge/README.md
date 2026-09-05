# Idle RPG Figma UI Bridge

AI writes a command to `commands/latest.json`. The Figma plugin applies it to the current file. A user can edit the generated layers and export the Home spec back to Unity.

## 1. Start the local bridge

From the repository root:

```powershell
python tools/figma-ui-bridge/server.py
```

The bridge listens on `http://127.0.0.1:8787`.

## 2. Install the Figma plugin

1. Open Figma.
2. Go to **Plugins → Development → Import plugin from manifest**.
3. Select `tools/figma-ui-bridge/manifest.json`.
4. Run **Idle RPG UI Bridge**.
5. Click **Apply AI command** once. The plugin then keeps polling the bridge, so later terminal commands apply automatically.

The plugin creates a `Home` frame with five level cards and named layers.

## 3. Let the user edit

Edit text, colors, spacing, or card order in the `Home` frame.

Keep these names/metadata when possible:

- `Home`
- `Title`
- `Subtitle`
- `Hint`
- `LevelGrid`
- `Level1` through `Level5`
- `Label` inside each level card

## 4. Apply the result to Unity

Click **Export Home to Unity**. The plugin writes:

```text
eoe/Assets/Resources/UI/ui-spec.json
```

Unity reimports the file. Re-enter Play Mode to see the updated text/button list.

## Terminal control

With the plugin open in the target Figma file, create or replace the Home frame from a terminal:

```powershell
python tools/figma-ui-bridge/cli.py build-home
python tools/figma-ui-bridge/cli.py build-home --title "IDLE // CIRCUIT" --subtitle "SELECT A LEVEL"
```

The plugin polls every 800 ms and applies a new command without another button click. Figma still requires the plugin to be started once per file/session because the Plugin API runs inside Figma's sandbox.

## AI command format

The AI can update `commands/latest.json` and increment `id`; the running plugin detects and applies it automatically.

This MVP syncs the Home screen structure and text. It intentionally does not convert every Figma constraint, effect, or animation into UGUI yet.
