from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import parse_qs, urlparse
import json
import time

ROOT = Path(__file__).resolve().parents[2]
COMMAND_PATH = Path(__file__).with_name("commands") / "latest.json"
UNITY_SPEC_PATH = ROOT / "Assets" / "Resources" / "UI" / "ui-spec.json"
EXPORT_PATH = Path(__file__).with_name("exports") / "figma-ui-spec.json"
last_plugin_seen = 0.0


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        global last_plugin_seen
        request = urlparse(self.path)
        if request.path == "/command":
            if "figma-plugin" in parse_qs(request.query).get("client", []):
                last_plugin_seen = time.time()
            self.send_json(json.loads(COMMAND_PATH.read_text(encoding="utf-8")))
            return

        if request.path == "/status":
            self.send_json({"connected": time.time() - last_plugin_seen < 3, "lastPluginSeen": last_plugin_seen})
            return

        self.send_error(404)

    def do_OPTIONS(self):
        self.send_response(204)
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "Content-Type")
        self.end_headers()

    def do_POST(self):
        if self.path != "/export":
            self.send_error(404)
            return

        length = int(self.headers.get("Content-Length", "0"))
        spec = json.loads(self.rfile.read(length).decode("utf-8"))
        payload = json.dumps(spec, indent=2, ensure_ascii=False) + "\n"
        EXPORT_PATH.parent.mkdir(parents=True, exist_ok=True)
        UNITY_SPEC_PATH.parent.mkdir(parents=True, exist_ok=True)
        EXPORT_PATH.write_text(payload, encoding="utf-8")
        UNITY_SPEC_PATH.write_text(payload, encoding="utf-8")
        self.send_json({"ok": True})

    def send_json(self, value):
        data = json.dumps(value, ensure_ascii=False).encode("utf-8")
        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(data)))
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(data)

    def log_message(self, format, *args):
        print(format % args)


if __name__ == "__main__":
    print("Figma bridge listening on http://127.0.0.1:8787")
    ThreadingHTTPServer(("127.0.0.1", 8787), Handler).serve_forever()
