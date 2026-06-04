# Cirreum.Contracts Changelog

All notable changes to **Cirreum.Contracts** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For detailed migration steps on major version bumps, see the per-version migration
guides linked at the bottom of each entry.

---

## [Unreleased]

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
