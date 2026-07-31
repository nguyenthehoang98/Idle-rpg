# ADR-0004 - Asset Loading: Addressables Local

Status: `accepted`

Date: 2026-07-31

## Context

Cần strategy để load asset (prefab, config JSON, sprite) trong game. Unity có 3 cách chính:
- **Resources**: deprecated, không khuyến khích
- **AssetBundles**: thủ công, cần build pipeline riêng
- **Addressables**: Unity recommended, async, memory management

## Decision

Dùng **Addressables Local** (`LocalBundleLoader`) cho MVP hiện tại. Cloud loading (`CloudBundleLoader`) để dành cho production.

## Rationale

- **Unity recommended**: Addressables là hệ thống asset management chính thức của Unity
- **Async loading**: hỗ trợ UniTask qua `AssetManager.GetAsset<T>("key")`
- **Memory management**: Addressables tự quản lý reference counting, release asset khi không dùng
- **Local đủ cho MVP**: assets nằm trong build, không cần download remote
- **Cloud-ready**: `AssetManager` có strategy pattern `SetAssetLocal()` / `SetAssetCloud()`, dễ chuyển sau
- **Đã implement**: `AssetManager` + `LocalBundleLoader` hoạt động trong project

API:
```csharp
AssetManager.SetAssetLocal();
GameObject prefab = await AssetManager.GetAssetCached<GameObject>("monster.1001");
```

## Consequences

- Cần setup Addressable Groups và build Addressables content
- Prefab phải được assign Addressable key
- `UnCache()` cần gọi khi không dùng nữa (tránh memory leak)
