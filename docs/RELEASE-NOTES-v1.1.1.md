# Cirreum.Contracts 1.1.1

## Summary

The caching configuration surface goes **code-first**: a provider is chosen by the
registration call, not an appsettings switch. This removes the `CacheProvider` enum and
`CacheSettings.Provider`, renames two cache types, and moves the app-author configuration
types into a dedicated `Cirreum.Caching.Configuration` namespace.

These are **breaking** changes. They ship as a **pre-adoption patch** (`1.1.0 → 1.1.1`,
via `-AllowBreakingPatch`) because there are essentially no consumers of the caching
surface yet — this is the first step of finalizing the caching foundation bottom-up.

## Why

The old `Cirreum:Cache:Provider` appsettings knob was redundant: it could never select a
provider whose package wasn't referenced, so the real selection has always been which
`Add…CacheService` you call. Making the registration call the single source of truth
removes a config option that could only ever be wrong or no-op, and separates
app-author *configuration* types from the runtime caching surface.

## What changed

### Removed

- **`CacheProvider` enum** and **`CacheSettings.Provider`.** Provider selection is now the
  registration call: `AddCirreumCaching` for the base / no-op, then
  `AddInMemoryCacheService` / `AddHybridCacheService` / `AddDistributedCacheService` to opt
  into a provider. `CacheSettings` keeps `DefaultExpiration` and TTL tuning.

### Changed — find / replace

| 1.1.0 | 1.1.1 | Notes |
|---|---|---|
| `QueryCacheOverride` | `CacheExpirationOverride` | It is the general cache-expiration override (global default + per-consumer maps), not query-specific |
| `CacheExpirationSettings` | `CacheExpirationPolicy` | The immutable per-operation expiration spec passed to `ICacheService` at runtime — not configuration; also avoids the clash with `ICacheableOperation.CacheExpiration` |
| `namespace Cirreum.Caching` <br>(for `CacheSettings`, `CacheExpirationOverride`) | `namespace Cirreum.Caching.Configuration` | Separates app-author *configuration* types from the runtime caching surface (matches the `Cirreum.Conductor.Configuration` convention) |

- `ICacheService` documentation reworded to reflect code-first provider selection.

## Migration

1. Delete any `Cirreum:Cache:Provider` appsettings entry — it no longer binds.
2. Select the provider in code: `AddCirreumCaching()` then the matching
   `Add…CacheService()` (from `Cirreum.Cache.InMemory` / `.Hybrid` / `.Distributed`).
3. Apply the find/replace table above.
4. Add `using Cirreum.Caching.Configuration;` where you configure `CacheSettings` /
   `CacheExpirationOverride`.

## Compatibility

- **Breaking, shipped as a patch** (`-AllowBreakingPatch`) — justified by essentially
  zero consumers of the caching surface pre-adoption.
- **Dependencies unchanged:** `Cirreum.Kernel 1.0.1`, `Cirreum.Result 2.0.0`.
- Runtime caching types stay in `Cirreum.Caching`; only the app-author configuration
  types moved to `Cirreum.Caching.Configuration`.
