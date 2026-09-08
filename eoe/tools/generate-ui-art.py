from __future__ import annotations

import math
import struct
import zlib
from pathlib import Path

W, H = 256, 256
OUT = Path(__file__).resolve().parents[1] / "Assets/Resources/UI/Art"


def rgba(hex_value: str, alpha: int = 255) -> tuple[int, int, int, int]:
    value = hex_value.lstrip("#")
    return (*bytes.fromhex(value), alpha)


def inside_round(x: int, y: int, box: tuple[int, int, int, int], radius: int) -> bool:
    left, top, right, bottom = box
    cx = min(max(x, left + radius), right - radius)
    cy = min(max(y, top + radius), bottom - radius)
    return (x - cx) ** 2 + (y - cy) ** 2 <= radius * radius


def set_pixel(pixels: list[list[tuple[int, int, int, int]]], x: int, y: int, color: tuple[int, int, int, int]) -> None:
    if 0 <= x < len(pixels[0]) and 0 <= y < len(pixels):
        pixels[y][x] = color


def rounded_fill(
    pixels: list[list[tuple[int, int, int, int]]],
    box: tuple[int, int, int, int],
    radius: int,
    top: tuple[int, int, int, int],
    bottom: tuple[int, int, int, int],
) -> None:
    left, top_y, right, bottom_y = box
    for y in range(max(0, top_y), min(len(pixels), bottom_y + 1)):
        t = (y - top_y) / max(1, bottom_y - top_y)
        color = tuple(round(top[i] * (1 - t) + bottom[i] * t) for i in range(4))
        for x in range(max(0, left), min(len(pixels[0]), right + 1)):
            if inside_round(x, y, box, radius):
                set_pixel(pixels, x, y, color)


def rounded_outline(
    pixels: list[list[tuple[int, int, int, int]]],
    box: tuple[int, int, int, int],
    radius: int,
    color: tuple[int, int, int, int],
    width: int,
) -> None:
    left, top, right, bottom = box
    for y in range(max(0, top), min(len(pixels), bottom + 1)):
        for x in range(max(0, left), min(len(pixels[0]), right + 1)):
            if inside_round(x, y, box, radius) and not inside_round(x, y, (left + width, top + width, right - width, bottom - width), max(0, radius - width)):
                set_pixel(pixels, x, y, color)


def line(pixels: list[list[tuple[int, int, int, int]]], start: tuple[int, int], end: tuple[int, int], color: tuple[int, int, int, int], width: int = 1) -> None:
    x0, y0 = start
    x1, y1 = end
    dx, dy = abs(x1 - x0), -abs(y1 - y0)
    sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
    error = dx + dy
    while True:
        for ox in range(-width // 2, width // 2 + 1):
            for oy in range(-width // 2, width // 2 + 1):
                set_pixel(pixels, x0 + ox, y0 + oy, color)
        if x0 == x1 and y0 == y1:
            return
        twice = 2 * error
        if twice >= dy:
            error += dy
            x0 += sx
        if twice <= dx:
            error += dx
            y0 += sy


def circle(pixels: list[list[tuple[int, int, int, int]]], center: tuple[int, int], radius: int, color: tuple[int, int, int, int]) -> None:
    cx, cy = center
    for y in range(cy - radius, cy + radius + 1):
        for x in range(cx - radius, cx + radius + 1):
            if (x - cx) ** 2 + (y - cy) ** 2 <= radius * radius:
                set_pixel(pixels, x, y, color)


def png_bytes(pixels: list[list[tuple[int, int, int, int]]]) -> bytes:
    raw = b"".join(b"\x00" + bytes(channel for pixel in row for channel in pixel) for row in pixels)

    def chunk(kind: bytes, data: bytes) -> bytes:
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)

    height, width = len(pixels), len(pixels[0])
    return b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)) + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")


def save(name: str, pixels: list[list[tuple[int, int, int, int]]]) -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / f"{name}.png").write_bytes(png_bytes(pixels))


def blank(width: int = W, height: int = H) -> list[list[tuple[int, int, int, int]]]:
    return [[(0, 0, 0, 0) for _ in range(width)] for _ in range(height)]


def panel(selected: bool = False) -> list[list[tuple[int, int, int, int]]]:
    pixels = blank()
    rounded_fill(pixels, (12, 18, 244, 250), 28, rgba("#223554" if not selected else "#3A5E6B"), rgba("#101A2B" if not selected else "#172A3D"))
    rounded_outline(pixels, (12, 18, 244, 250), 28, rgba("#5EEAD4" if not selected else "#FBBF24", 210), 4)
    rounded_outline(pixels, (24, 30, 232, 238), 18, rgba("#F8FAFC", 24), 2)
    line(pixels, (38, 34), (218, 34), rgba("#F8FAFC", 46), 3)
    line(pixels, (38, 232), (218, 232), rgba("#020617", 150), 6)
    return pixels


def button(kind: str) -> list[list[tuple[int, int, int, int]]]:
    pixels = blank(512, 128)
    colors = {
        "primary": ("#B8FFF3", "#159A98", "#D8FFFA"),
        "secondary": ("#3A5579", "#1D2C46", "#90A9CB"),
        "disabled": ("#64748B", "#334155", "#94A3B8"),
        "danger": ("#FDA4AF", "#BE123C", "#FB7185"),
    }
    top, bottom, edge = colors[kind]
    rounded_fill(pixels, (8, 8, 504, 116), 22, rgba(top), rgba(bottom))
    rounded_outline(pixels, (8, 8, 504, 116), 22, rgba(edge, 190), 3)
    line(pixels, (38, 24), (474, 24), rgba("#FFFFFF", 80 if kind == "primary" else 30), 5)
    line(pixels, (34, 100), (478, 100), rgba("#020617", 100), 5)
    return pixels


def badge(kind: str) -> list[list[tuple[int, int, int, int]]]:
    pixels = blank(256, 64)
    color = "#5EEAD4" if kind == "accent" else "#FBBF24"
    rounded_fill(pixels, (3, 3, 253, 61), 29, rgba(color, 42), rgba(color, 42))
    rounded_outline(pixels, (3, 3, 253, 61), 29, rgba(color, 220), 3)
    line(pixels, (28, 17), (228, 17), rgba("#FFFFFF", 35), 3)
    return pixels


def slot(active: bool) -> list[list[tuple[int, int, int, int]]]:
    pixels = blank()
    if not active:
        rounded_fill(pixels, (7, 7, 249, 249), 34, rgba("#07111F"), rgba("#07111F"))
        rounded_outline(pixels, (7, 7, 249, 249), 34, rgba("#5B7592", 210), 4)
        rounded_outline(pixels, (25, 25, 231, 231), 24, rgba("#A8B5C7", 36), 3)
        for x in range(86, 174, 16):
            line(pixels, (x, 128), (min(x + 8, 174), 128), rgba("#A8B5C7", 90), 6)
        for y in range(86, 174, 16):
            line(pixels, (128, y), (128, min(y + 8, 174)), rgba("#A8B5C7", 90), 6)
    else:
        rounded_fill(pixels, (7, 7, 249, 249), 34, rgba("#203B55"), rgba("#203B55"))
        rounded_outline(pixels, (7, 7, 249, 249), 34, rgba("#5EEAD4"), 5)
        rounded_outline(pixels, (24, 24, 232, 232), 24, rgba("#D8FFFA", 46), 3)
        circle(pixels, (128, 128), 63, rgba("#5EEAD4", 26))
        line(pixels, (128, 84), (150, 128), rgba("#5EEAD4", 190), 7)
        line(pixels, (150, 128), (128, 172), rgba("#5EEAD4", 190), 7)
        line(pixels, (128, 172), (106, 128), rgba("#5EEAD4", 190), 7)
        line(pixels, (106, 128), (128, 84), rgba("#5EEAD4", 190), 7)
    return pixels


def main() -> None:
    save("ui-panel", panel())
    save("ui-panel-selected", panel(True))
    save("ui-button-primary", button("primary"))
    save("ui-button-secondary", button("secondary"))
    save("ui-button-disabled", button("disabled"))
    save("ui-button-danger", button("danger"))
    save("ui-badge-accent", badge("accent"))
    save("ui-badge-warning", badge("warning"))
    save("ui-slot-empty", slot(False))
    save("ui-slot-active", slot(True))
    divider = blank(512, 8)
    line(divider, (4, 4), (508, 4), rgba("#5EEAD4", 70), 8)
    line(divider, (4, 4), (508, 4), rgba("#5EEAD4", 220), 2)
    save("ui-divider-accent", divider)


if __name__ == "__main__":
    main()
