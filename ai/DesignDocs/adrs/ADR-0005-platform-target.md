# ADR-0005 - Platform Target: Mobile

Status: `accepted`

Date: 2026-07-31

## Context

Quyết định platform ưu tiên cho game TDSurvivor. Lựa chọn giữa PC (desktop) hoặc Mobile (iOS + Android).

## Decision

**Mobile** (iOS + Android), hỗ trợ editor testing với `SafeArea` + `SimDevice`.

## Rationale

- **Interview D07**: đã chốt mobile target trong interview
- **Gameplay phù hợp mobile**: auto-attack, không cần complex input, session ngắn 5-10 phút
- **SafeArea đã có**: `_GameToolkit.Utils.SafeArea` hỗ trợ notch devices (iPhone X, Xs Max, Pixel 3 XL...)
- **Input đơn giản**: auto-attack + force direction (tap/swipe) không cần virtual joystick
- **Unity 6 URP 2D**: tối ưu tốt cho mobile rendering

## Consequences

- Performance budget thấp hơn PC (cần tối ưu draw call, pool, particles)
- Cần test real device (không chỉ editor)
- Aspect ratio handling (16:9, 19.5:9, tablet...)
- Pin/Battery: game loop (fixed-timestep 30fps) cần optimized
- UI scaling: Canvas Scaler phải hỗ trợ nhiều resolution
