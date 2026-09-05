"""Send UI commands to the local Figma plugin bridge."""

from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path
from urllib.request import urlopen

COMMAND_PATH = Path(__file__).with_name("commands") / "latest.json"
BRIDGE_URL = "http://localhost:8787"


def check_status(_args: argparse.Namespace) -> None:
    print(urlopen(f"{BRIDGE_URL}/status", timeout=3).read().decode("utf-8"))


def build_home(args: argparse.Namespace) -> None:
    command = json.loads(COMMAND_PATH.read_text(encoding="utf-8"))
    command["id"] = args.id or datetime.now(timezone.utc).strftime("home-%Y%m%d%H%M%S%f")
    command["type"] = "build_home"
    spec = command.setdefault("spec", {})
    for name in ("title", "subtitle", "hint"):
        value = getattr(args, name)
        if value is not None:
            spec[name] = value

    temporary = COMMAND_PATH.with_suffix(".tmp")
    temporary.write_text(json.dumps(command, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    temporary.replace(COMMAND_PATH)
    print(f"Queued {command['id']} -> {COMMAND_PATH}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    subparsers = parser.add_subparsers(dest="command", required=True)
    status = subparsers.add_parser("status", help="check whether the Figma plugin polled the bridge")
    status.set_defaults(run=check_status)
    home = subparsers.add_parser("build-home", help="create or replace the Home UI frame")
    home.add_argument("--title")
    home.add_argument("--subtitle")
    home.add_argument("--hint")
    home.add_argument("--id")
    home.set_defaults(run=build_home)
    args = parser.parse_args()
    args.run(args)


if __name__ == "__main__":
    main()
