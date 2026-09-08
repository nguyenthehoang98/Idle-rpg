from __future__ import annotations

import argparse
import json
import time
import urllib.request
from pathlib import Path

BASE_URL = "http://127.0.0.1:38471"


def request(path: str, token: str, method: str = "GET", payload: dict | None = None) -> dict:
    data = None if payload is None else json.dumps(payload, separators=(",", ":")).encode("utf-8")
    headers = {"X-Bridge-Token": token}
    if data is not None:
        headers["Content-Type"] = "application/json"
    request = urllib.request.Request(BASE_URL + path, data=data, headers=headers, method=method)
    with urllib.request.urlopen(request, timeout=5) as response:
        return json.loads(response.read())


def main() -> int:
    parser = argparse.ArgumentParser(description="Run one Figma UI batch and wait for its result")
    parser.add_argument("batch_file", type=Path)
    parser.add_argument("--timeout", type=float, default=120)
    args = parser.parse_args()

    commands = json.loads(args.batch_file.read_text(encoding="utf-8"))
    if not isinstance(commands, list) or not 1 <= len(commands) <= 50:
        raise SystemExit("Batch must contain 1-50 command objects")
    token_file = Path(__file__).with_name(".bridge-token")
    token = token_file.read_text(encoding="utf-8").strip()
    accepted = request("/command", token, "POST", {"op": "batch", "args": {"commands": commands}})
    command_id = accepted["id"]
    deadline = time.monotonic() + args.timeout
    while time.monotonic() < deadline:
        time.sleep(0.5)
        events = request("/events", token)["events"]
        event = next((item for item in events if item.get("id") == command_id), None)
        if event is not None:
            print(json.dumps(event["result"], indent=2))
            return 0 if event["result"].get("ok") else 1
    raise SystemExit(f"Timed out waiting for Figma batch {command_id}; keep the plugin connected")


if __name__ == "__main__":
    raise SystemExit(main())
