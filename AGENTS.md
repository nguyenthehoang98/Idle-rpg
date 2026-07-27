# EOE Project — AGENTS.md

## Project
Unity 6000.2.13f1 (C# 9, .NET Standard 2.1, Mono backend, Gamma color space).  
Entry: `BootScene` → `GameEntry` (extends `KitEntryScene`).  
`Application.targetFrameRate = 60; vSyncCount = 0` set in `KitEntryScene.Awake()`.

## Structure
- `_KITSystem/` — framework asmdefs: `Entity` (standalone), `Config` → `Resource` (standalone, *no dep on Entity*), `Schedule` (standalone), `Utils` (standalone)
- `_KITSystem/SkillSystem/Core/` + `_KITSystem/SkillSystem/Imp/` — **no asmdef → fall into Assembly-CSharp** alongside `_Game/` code
- `_Game/` — game code, **no asmdef → all in default Assembly-CSharp** (slow compile, auto-references all plugins)

## Assembly Dependencies
```
Assembly-CSharp (_Game/ + SkillSystem/)
    → KITSystem.Config → KITSystem.Resource → (packages)
    → KITSystem.Entity (standalone)
    → KITSystem.Schedule (standalone)
    → KITSystem.Utils (standalone)
```
All `_Game/` code and all `_KITSystem/SkillSystem/` code live in Assembly-CSharp (no asmdef separation).

## Build/Test
- .NET Standard 2.1, Mono backend (IL2CPP not enabled for Android).
- Only LitMotion has test assemblies (NUnit, `UNITY_INCLUDE_TESTS`). **No game tests exist.**
- `nunit.framework` auto-referenced by all assemblies via `com.unity.ext.nunit`.
- PlayMode test runner disabled. EditMode works — place `[Test]` files guarded by `#if UNITY_INCLUDE_TESTS`.

## Key Patterns
- **Custom ECS** (not DOTS): `EntityManager` + `ComponentManager<T>` (`T : unmanaged`). Sparse-set storage. Queries via `Query<T>()`.
- **Spu** (Skill Processing Unit): command-queue pattern (`RequestAddAction` → `FlushCommands`). State-change guard in `#if UNITY_EDITOR`.
- **Config pipeline**: Google Sheets → JSON → LZ4 compress → `AssetBundleManager`. Runtime: LZ4 decompress → `JsonUtility` → `IGameConfig.OnMappingValue()`. `OnPostImported()` is **Editor-only** (called by `ConfigDownloaderWindow`, not by `ConfigManager.Load()`).
- **Tick system**: `TickSystemOwner` (MonoBehaviour) → fixed-timestep accumulator (30 FPS default) → `ITickable.Tick(float)`.
- **Pool**: Singleton MonoBehaviour, keyed by `GameObject.name`. Overrides `Instantiate`/`Destroy`.

## Conventions
- Namespaces: `_KITSystem.*`, `_Game.*`. No rootNamespace in asmdefs.
- `[SerializeField] private` PascalCase (no `_` prefix). Structs for pure data.
- `#if UNITY_EDITOR` for editor-only code. `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on hot paths.
- Vietnamese comments in some files (Spu.cs, EntityManager.cs, GameManager.cs).

## Risk Notes
- **CircleCollider hitbox**: `size = (radius / 2f, radius / 2f)` → effective collision area is smaller than expected radius.
- **`OnPostImported()` not called at runtime**: MonsterConfig scale×base merge and LevelConfig wave→spawn cross-ref resolution run in Editor only.
- **SkillSystem has no asmdef**: Falls into Assembly-CSharp — no editor/runtime separation and slow compile.
- **Config JSON files are LZ4-compressed**: Cannot edit raw JSON directly; use `ConfigDownloaderWindow`.
