from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Any

DEFAULT_SPEC = Path(__file__).parents[1] / "asset-lab/specs/home/home.v001.json"
DEFAULT_OUTPUT = Path(__file__).parents[1] / "figma-bridge/batches/home-spec-v001.json"
MAX_BATCH_COMMANDS = 50


def hex_color(value: str) -> dict[str, float]:
    if not isinstance(value, str) or len(value) != 7 or not value.startswith("#"):
        raise ValueError(f"Expected #RRGGBB color, got {value!r}")
    try:
        channels = [int(value[index : index + 2], 16) / 255 for index in (1, 3, 5)]
    except ValueError as exc:
        raise ValueError(f"Invalid color {value!r}") from exc
    return dict(zip(("r", "g", "b"), channels, strict=True))


def validate_spec(spec: dict[str, Any]) -> None:
    if spec.get("schema") != "idle-rpg-ui/v1":
        raise ValueError("Unsupported UI spec schema")
    if spec.get("screen") != "home":
        raise ValueError("This renderer only supports the Home screen")
    position = spec.get("figmaPosition", {})
    if not all(isinstance(position.get(axis), (int, float)) for axis in ("x", "y")):
        raise ValueError("Home spec must define a numeric Figma preview position")
    canvas = spec.get("canvas", {})
    if canvas.get("width") != 1080 or canvas.get("height") != 2400:
        raise ValueError("Home spec must use the 1080x2400 portrait canvas")
    components = spec.get("components")
    if not isinstance(components, list) or not components:
        raise ValueError("Home spec must contain components")
    ids = [component.get("id") for component in components]
    if any(not component_id for component_id in ids) or len(ids) != len(set(ids)):
        raise ValueError("Component ids must be present and unique")
    if not any(component.get("id") == "level-01-card" for component in components):
        raise ValueError("Home spec must contain the available Level 01 card")
    if not any(component.get("id") == "continue-button" for component in components):
        raise ValueError("Home spec must contain the primary CTA")


def color_for(spec: dict[str, Any], token: str) -> dict[str, float]:
    tokens = spec["tokens"]
    if token not in tokens:
        raise ValueError(f"Unknown color token {token!r}")
    return hex_color(tokens[token])


def build_batch(spec: dict[str, Any]) -> list[dict[str, Any]]:
    validate_spec(spec)
    commands: list[dict[str, Any]] = [
        {
            "op": "create_frame",
            "as": "home",
            "args": {
                "name": f"Home / Spec {spec['version']}",
                "width": spec["canvas"]["width"],
                "height": spec["canvas"]["height"],
                "x": spec["figmaPosition"]["x"],
                "y": spec["figmaPosition"]["y"],
            },
        }
    ]

    for component in spec["components"]:
        args = {"name": component["name"], "parentId": "$home"}
        if component["kind"] == "rectangle":
            args.update(component["rect"])
            commands.append({"op": "create_rectangle", "as": component["id"], "args": args})
            commands.append({
                "op": "set_fill",
                "args": {"nodeId": f"${component['id']}", "color": color_for(spec, component["fill"]), "opacity": 1},
            })
            if "stroke" in component:
                stroke = component["stroke"]
                commands.append({
                    "op": "set_stroke",
                    "args": {
                        "nodeId": f"${component['id']}",
                        "color": color_for(spec, stroke["color"]),
                        "opacity": stroke["opacity"],
                        "weight": stroke["weight"],
                    },
                })
            if "radius" in component:
                commands.append({
                    "op": "set_corner_radius",
                    "args": {"nodeId": f"${component['id']}", "radius": component["radius"]},
                })
        elif component["kind"] == "text":
            position = component["position"]
            args.update({"text": component["text"], "fontSize": component["fontSize"], **position})
            commands.append({"op": "create_text", "as": component["id"], "args": args})
            commands.append({
                "op": "set_fill",
                "args": {"nodeId": f"${component['id']}", "color": color_for(spec, component["fill"]), "opacity": 1},
            })
        else:
            raise ValueError(f"Unsupported component kind {component['kind']!r}")

    if len(commands) > MAX_BATCH_COMMANDS:
        raise ValueError(f"Generated batch has {len(commands)} commands; limit is {MAX_BATCH_COMMANDS}")
    return commands


def main() -> None:
    parser = argparse.ArgumentParser(description="Render the canonical Home spec into a Figma bridge batch")
    parser.add_argument("--spec", type=Path, default=DEFAULT_SPEC)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()

    spec = json.loads(args.spec.read_text(encoding="utf-8"))
    batch = build_batch(spec)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(batch, indent=2) + "\n", encoding="utf-8")
    print(f"wrote {len(batch)} commands to {args.output}")


if __name__ == "__main__":
    main()
