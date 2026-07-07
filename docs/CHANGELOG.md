# Cirreum.Contracts Changelog

All notable changes to **Cirreum.Contracts** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For detailed migration steps on major version bumps, see the per-version migration
guides linked at the bottom of each entry.

---

## [Unreleased]

### Updated

- Updated NuGet packages.

## [1.4.1] - 2026-07-05

### Fixed

- Documented `IConnectionLifecycle`'s live-state-vs-cleanup division: `OnConnectedAsync`/
  `OnDisconnectedAsync` bracket the connection's *live* state, so `OnDisconnectedAsync` fires only for a
  connection that actually went live — a rejected or faulted establishment never receives it (its
  `DisconnectInfo` would be meaningless). Cleanup that must survive rejection/fault binds to
  `IInvocationConnection.Aborted` instead, which cancels on every teardown path. No behavioral change —
  this makes the existing, intended contract explicit (the framework's connection registry already
  follows it).

## [1.4.0] - 2026-07-05

### Added

- **`PromotedUser`** (`ClaimsPrincipal?`) joins the connection-ownership surface — the nullable
  primitive behind `EffectiveUser`/`IsUserPromoted`, for consumers that need the promoted identity
  *as distinct from* the upgrade-time `User` (audit trails and diagnostics reporting both identities
  of a promoted connection). Fully replaces `TwoPhaseAuth.GetPromotedPrincipal`.

### Changed

- **`EffectiveUser` + `IsUserPromoted` are now extension members** (`InvocationConnectionExtensions`,
  C# 14 extension syntax), replacing the default interface members shipped in `1.3.0` hours earlier.
  Same call-site syntax (`connection.EffectiveUser`) and identical semantics, but without the two DIM
  quirks: the members are visible on concrete implementing types without an interface cast (DIMs never
  join the class's surface), and test doubles get the real logic for free (dynamic-proxy mocks override
  DIMs, silently bypassing the default body). Nothing of value was lost — per-implementation override
  of connection-ownership resolution was a liability, not a feature: every transport must answer "who
  owns this connection" identically.

> Technically breaking against `1.3.0`'s interface surface (the two DIMs are removed), shipped hours
> after `1.3.0` with zero consumers of the DIM form.

## [1.3.0] - 2026-07-05

### Added

- **`IInvocationConnection.EffectiveUser` + `IsUserPromoted`** (default interface members) — the
  connection's *effective* principal: the Two-Phase Auth promoted principal when one has been stamped
  into `Items` (under `AuthenticationContextKeys.PromotedPrincipal`), otherwise the upgrade-time
  `User`. Establishes "who owns this connection *now*" as first-class contract surface instead of a
  magic Items key every consumer had to know about — the per-invocation contexts, the connection
  registry's subject lookup, the connection-terminator's session matching, and app lifecycle
  hooks/diagnostics all read the same member. `User` stays immutable post-upgrade; promotion decorates
  the connection through `Items`, and re-promotion overwrites, so `EffectiveUser` always reflects the
  most recent promotion. Additive: existing implementers inherit the defaults.

### Fixed

- `IInvocationConnectionRegistry`'s docs placed the framework's connection-terminator in
  `Cirreum.Runtime.Server` and named only two auth events; the terminator ships in
  `Cirreum.Services.Server` (ADR-0027 Phase B) and also reacts to `UserAccountDisabled`. Also
  documented that `FindBySubject` resolves subjects from `EffectiveUser`, so promoted connections
  match under their promoted identity.

## [1.2.1] - 2026-07-05

### Updated

- Updated NuGet packages.

## [1.2.0] - 2026-07-04

### Added

- **`IUserProfileEnrichmentBuilder`, `IGraphEnabledBuilder`, `IExternalGraphEnabledBuilder`** — the profile-enrichment builder family, relocated here from `Cirreum.AuthenticationProvider`. These are host-agnostic (any host may enrich a user's profile after authentication, regardless of which — or whether any — auth scheme is active), the same shape of variance `IUserPresenceBuilder` already has here. `IUserProfileEnrichmentBuilder` no longer extends `IAuthenticationBuilder` — it never needed that interface's server-only `AuthBuilder`/`Configuration` members, and inheriting them silently broke every Blazor WebAssembly implementer (there is no server-side `AuthenticationBuilder` on a WASM client). Default enricher implementations ship in `Cirreum.Domain`.

## [1.1.1] - 2026-07-03

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
