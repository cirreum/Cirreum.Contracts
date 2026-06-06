# Cirreum.Contracts Changelog

All notable changes to **Cirreum.Contracts** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For detailed migration steps on major version bumps, see the per-version migration
guides linked at the bottom of each entry.

---

## [Unreleased]

### Removed

- **`CacheProvider` enum and `CacheSettings.Provider`.** Cache provider selection is now **code-first**:
  the registration call is the choice — `AddCirreumCaching` for the base/no-op, then
  `AddInMemoryCacheService` / `AddHybridCacheService` / `AddDistributedCacheService` to opt into a
  provider. The appsettings `Cirreum:Cache:Provider` knob was redundant (it could never select a provider
  whose package wasn't referenced) and is gone. `CacheSettings` keeps `DefaultExpiration` and TTL tuning.

### Changed

- **Renamed `QueryCacheOverride` → `CacheExpirationOverride`.** It is the general cache-expiration override
  (used for the global default `CacheSettings.DefaultExpiration` and per-consumer override maps such as the
  Conductor's per-query overrides), not query-specific.
- `ICacheService` documentation reworded to reflect code-first provider selection.
- **Moved `CacheSettings` and `CacheExpirationOverride` into the `Cirreum.Caching.Configuration`
  namespace** — separating app-author *configuration* types from the runtime caching surface (matches
  the `Cirreum.Conductor.Configuration` convention).
- **Renamed `CacheExpirationSettings` → `CacheExpirationPolicy`** — it is the immutable per-operation
  expiration spec passed to `ICacheService` at runtime, *not* configuration (the config-time shape is
  `CacheExpirationOverride`). The new name also avoids the clash with the
  `ICacheableOperation.CacheExpiration` property.

> These are breaking, shipped as a pre-adoption patch via `-AllowBreakingPatch` (essentially zero
> consumers). First step of the bottom-up caching-foundation finalize.

## [1.1.0] - 2026-06-05

### Changed

- Bumped `Cirreum.Result` `1.0.16` → `2.0.0`. This propagates the `Result`/`Result<T>`
  System.Text.Json round-trip fix (a serialized success no longer deserializes as a
  failure), the `IErrorState` opt-in error-state contract, the `SurrogateResultException`
  carrier + `HasError` matchers, and the rewritten pagination types
  (`SliceResult`/`CursorResult`/`PagedResult`). Contracts' own contract surface is
  unchanged; consumers that use the re-exposed pagination types via Contracts should
  review the `Cirreum.Result` 2.0.0 migration notes.

## [1.0.0] - 2026-06-04

### Added

- Initial release. Cirreum.Contracts is the contract surface of the Cirreum framework, established as part of the **Cirreum 1.0 Foundation Reset** wave.
- Absorbs cross-host content from former `Cirreum.Core 5.x`:
  - **Conductor** — CQRS dispatcher, publisher, intercepts (HandlerPerformance, QueryCaching, Validation), builders, telemetry, logging
  - **Caching** — `ICacheService`, `InMemoryCacheService`, `InstrumentedCacheService`, `NoCacheService`, `CacheProvider`, settings, telemetry
  - **RemoteServices** — `IRemoteConnection`, `RemoteClient`, `RemoteConnectionBase`, telemetry, options
  - **FileSystem** — `IFileSystem`, `IMauiHybridFileSystem`, `FileSystemUtils`, `PathType`; CSV helpers (`ICsvFileBuilder`, `ICsvFileReader`, `CsvOptions`); `SystemIOExtensions`
  - **State** — cross-host state abstractions and `ScopedNotificationState` concrete
  - **Presence** — `IUserPresenceService`, `IUserPresenceMonitor`, `UserPresence`, `PresenceStatus`, builder, options
  - **Result extensions** — `ResultExtensions` (FluentValidation → Result&lt;T&gt; glue)
  - **Authorization pillar** — the vocabulary (`AuthorizationPolicies`, `ApplicationRoles`, `Role`, `Permission`, `PermissionSet`, `DenyCodes`, `AuthorizationTelemetry`) plus the cross-host contracts and abstractions: `AuthorizationContext`, `IAuthorizationContextAccessor`, `AuthorizationDenial`, `IAuthorizationEvaluator`, `IAuthorizer`, `IAuthorizationRoleRegistry`, `IRoleDefinitionProvider`, `IAuthorizableObject`, `RequiresGrantAttribute`, `RequiredGrantCache`, `AuthorizationIntercept`, and the `Operations/`, `Operations/Grants/`, and `Resources/` contract sets. The default implementations + FluentValidation validators live in `Cirreum.Domain`; no Authorization content remains in `Cirreum.Kernel`.
- DI extensions per feature folder travel with the feature (Conductor, Cache).

### Migration

Apps consuming the absorbed content from `Cirreum.Core 5.x` migrate by installing `Cirreum.Contracts`. Namespace `Cirreum.Conductor.*`, `Cirreum.Caching.*`, `Cirreum.RemoteServices.*`, `Cirreum.FileSystem.*`, `Cirreum.State.*`, `Cirreum.Presence.*`, `Cirreum.Authorization.*` preserved.
