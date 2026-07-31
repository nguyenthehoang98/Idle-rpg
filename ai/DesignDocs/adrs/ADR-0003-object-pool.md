# ADR-0003 - Object Pool Strategy

Status: `accepted`

Date: 2026-07-31

## Context

Game spawn hàng trăm monster mỗi wave, cộng thêm projectile từ hero. Nếu dùng `Instantiate()`/`Destroy()` trực tiếp sẽ gây GC spike, ảnh hưởng performance mobile.

Cần chọn object pool strategy phù hợp.

## Decision

Dùng `UnityEngine.Pool.ObjectPool<T>` wrapped bởi `_GameToolkit.Resource.Pool` singleton.

## Rationale

- **Đã implement**: Pool đã có trong GameToolkit, battle-tested từ Recovery
- **Unity built-in**: `UnityEngine.Pool.ObjectPool` được Unity maintain, không cần third-party
- **Key-by-name**: `RegisterPool(GameObject)` key bằng gameObject name → đơn giản, dễ dùng
- **Scene cleanup**: `RegisterPool(go, isDestroyIfChangeScene: true)` → tự cleanup khi chuyển scene
- **Editor debugging**: `PoolViewerWindow` (Tools/Engine/Pool Viewer) giúp theo dõi pool status

API chính:
```csharp
Pool.RegisterPool(prefab);
Monster m = Pool.Instantiate(prefab);
Pool.Destroy(m.gameObject); // return to pool
```

## Consequences

- Cần gọi `Pool.RegisterPool()` trước khi spawn (thực hiện trong `SpawnerUpdater.Initialize()`)
- GameObject phải reset state khi return pool (reset HP, position, active state...)
- Pool.Internal dùng `UnityEngine.Pool.ObjectPool` với defaultCapacity, maxSize
