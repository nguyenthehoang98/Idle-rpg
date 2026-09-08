from __future__ import annotations

import argparse
import json
import os
import secrets
import sys
import threading
from collections import deque
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any
from urllib.parse import parse_qs, urlparse

HOST = "127.0.0.1"
PORT = 38471
MAX_BODY = 64 * 1024
MAX_QUEUE = 100
ALLOWED_OPERATIONS = {
    "create_page",
    "create_frame",
    "create_rectangle",
    "create_text",
    "set_fill",
    "set_stroke",
    "set_corner_radius",
    "set_name",
    "set_gradient",
    "set_effects",
    "set_rotation",
    "select",
    "ping",
    "batch",
}
TOKEN_FILE = Path(__file__).with_name(".bridge-token")


class BridgeState:
    def __init__(self, token: str) -> None:
        self.token = token
        self.commands: deque[dict[str, Any]] = deque(maxlen=MAX_QUEUE)
        self.events: deque[dict[str, Any]] = deque(maxlen=MAX_QUEUE)
        self.lock = threading.Lock()

    def enqueue(self, command: dict[str, Any]) -> None:
        with self.lock:
            self.commands.append(command)

    def next_command(self) -> dict[str, Any] | None:
        with self.lock:
            return self.commands.popleft() if self.commands else None

    def add_event(self, event: dict[str, Any]) -> None:
        with self.lock:
            self.events.append(event)

    def snapshot(self) -> dict[str, int]:
        with self.lock:
            return {"queuedCommands": len(self.commands), "recentEvents": len(self.events)}


def load_token() -> str:
    configured = os.environ.get("FIGMA_BRIDGE_TOKEN", "").strip()
    if configured:
        return configured
    if TOKEN_FILE.exists():
        token = TOKEN_FILE.read_text(encoding="utf-8").strip()
        if token:
            return token
    token = secrets.token_urlsafe(32)
    TOKEN_FILE.write_text(token + "\n", encoding="utf-8")
    return token


def json_response(handler: BaseHTTPRequestHandler, status: int, payload: dict[str, Any]) -> None:
    encoded = json.dumps(payload, separators=(",", ":")).encode("utf-8")
    handler.send_response(status)
    handler.send_header("Content-Type", "application/json")
    handler.send_header("Content-Length", str(len(encoded)))
    handler.send_header("Access-Control-Allow-Origin", "*")
    handler.send_header("Access-Control-Allow-Headers", "Content-Type, X-Bridge-Token")
    handler.send_header("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
    handler.end_headers()
    handler.wfile.write(encoded)


def error(code: str, message: str) -> dict[str, Any]:
    return {"ok": False, "error": {"code": code, "message": message}}


class Handler(BaseHTTPRequestHandler):
    server: "BridgeServer"

    def log_message(self, format: str, *args: Any) -> None:
        print(f"[bridge] {format % args}")

    def do_OPTIONS(self) -> None:
        json_response(self, 204, {})

    def do_GET(self) -> None:
        parsed = urlparse(self.path)
        if parsed.path == "/health":
            json_response(self, 200, {"ok": True, "service": "figma-local-bridge", **self.server.state.snapshot()})
            return
        if not self.authenticated(parsed.query):
            json_response(self, 401, error("UNAUTHORIZED", "Invalid or missing bridge token"))
            return
        if parsed.path == "/next":
            command = self.server.state.next_command()
            json_response(self, 200, {"ok": True, "command": command})
            return
        if parsed.path == "/events":
            with self.server.state.lock:
                events = list(self.server.state.events)
            json_response(self, 200, {"ok": True, "events": events})
            return
        json_response(self, 404, error("NOT_FOUND", "Unknown endpoint"))

    def do_POST(self) -> None:
        parsed = urlparse(self.path)
        if not self.authenticated(""):
            json_response(self, 401, error("UNAUTHORIZED", "Invalid or missing bridge token"))
            return
        try:
            length = int(self.headers.get("Content-Length", "0"))
        except ValueError:
            length = 0
        if length <= 0 or length > MAX_BODY:
            json_response(self, 413, error("BODY_TOO_LARGE", f"Body must be 1-{MAX_BODY} bytes"))
            return
        try:
            payload = json.loads(self.rfile.read(length))
        except (json.JSONDecodeError, UnicodeDecodeError):
            json_response(self, 400, error("INVALID_JSON", "Request body must be valid JSON"))
            return
        if not isinstance(payload, dict):
            json_response(self, 422, error("INVALID_PAYLOAD", "Request body must be an object"))
            return
        if parsed.path == "/command":
            operation = payload.get("op")
            if not isinstance(operation, str) or operation not in ALLOWED_OPERATIONS:
                json_response(self, 422, error("OPERATION_NOT_ALLOWED", "Operation is not in the allowlist"))
                return
            command_id = payload.get("id") or secrets.token_hex(8)
            if not isinstance(command_id, str) or len(command_id) > 80:
                json_response(self, 422, error("INVALID_ID", "Command id must be a short string"))
                return
            args = payload.get("args", {})
            if not isinstance(args, dict):
                json_response(self, 422, error("INVALID_ARGS", "Command args must be an object"))
                return
            self.server.state.enqueue({"id": command_id, "op": operation, "args": args})
            json_response(self, 202, {"ok": True, "id": command_id})
            return
        if parsed.path == "/event":
            self.server.state.add_event(payload)
            json_response(self, 202, {"ok": True})
            return
        json_response(self, 404, error("NOT_FOUND", "Unknown endpoint"))

    def authenticated(self, query: str) -> bool:
        supplied = self.headers.get("X-Bridge-Token", "")
        if not supplied:
            supplied = parse_qs(query).get("token", [""])[0]
        return secrets.compare_digest(supplied, self.server.state.token)


class BridgeServer(ThreadingHTTPServer):
    def __init__(self, token: str) -> None:
        self.state = BridgeState(token)
        super().__init__((HOST, PORT), Handler)


def command(args: argparse.Namespace) -> int:
    import urllib.request

    payload = {"op": args.operation, "args": json.loads(args.args)}
    request = urllib.request.Request(
        f"http://{HOST}:{PORT}/command",
        data=json.dumps(payload).encode("utf-8"),
        headers={"Content-Type": "application/json", "X-Bridge-Token": load_token()},
        method="POST",
    )
    try:
        with urllib.request.urlopen(request, timeout=3) as response:
            print(response.read().decode("utf-8"))
    except OSError as exc:
        print(f"bridge unavailable: {exc}", file=sys.stderr)
        return 1
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description="Loopback-only Figma command bridge")
    subparsers = parser.add_subparsers(dest="mode", required=True)
    serve_parser = subparsers.add_parser("serve")
    serve_parser.set_defaults(mode="serve")
    token_parser = subparsers.add_parser("token")
    token_parser.set_defaults(mode="token")
    command_parser = subparsers.add_parser("command")
    command_parser.add_argument("operation", choices=sorted(ALLOWED_OPERATIONS))
    command_parser.add_argument("--args", default="{}", help="JSON object")
    args = parser.parse_args()

    if args.mode == "token":
        print(load_token())
        return 0
    if args.mode == "command":
        return command(args)

    token = load_token()
    server = BridgeServer(token)
    print(f"Figma bridge listening on http://{HOST}:{PORT}")
    print(f"Token stored locally at {TOKEN_FILE}")
    print("To print the token in another terminal: python bridge.py token")
    print("No external network interface is opened. Press Ctrl+C to stop.")
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nBridge stopped.")
    finally:
        server.server_close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
