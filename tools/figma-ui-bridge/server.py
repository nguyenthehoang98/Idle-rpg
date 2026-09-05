from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[2]
COMMAND_PATH = Path(__file__).with_name("commands") / "latest.json"
UNITY_SPEC_PATH = ROOT / "Assets" / "Resources" / "UI" / "ui-spec.json"
EXPORT_PATH = Path(__file__).with_name("exports") / "figma-ui-spec.json"


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path != "/command":
            self.send_error(404)
            return

        self.send_json(json.loads(COMMAND_PATH.read_text(encoding="utf-8")))

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
