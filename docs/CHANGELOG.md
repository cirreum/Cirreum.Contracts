# Cirreum.Contracts Changelog

All notable changes to **Cirreum.Contracts** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For detailed migration steps on major version bumps, see the per-version migration
guides linked at the bottom of each entry.

---

## [Unreleased]

### Added

- **`IInvocationContext.AuthenticatedScheme`** — extension member reading the invocation's
  scheme stamp, uniform across HTTP and long-lived sources (HTTP invocations surface
  `HttpContext.Items` directly; connection invocations seed the slot from the connection's
  authentication state). Replaces the raw
  `Items[AuthenticationContextKeys.AuthenticatedScheme]` read at app call sites — session-ticket
  negotiate endpoints and anywhere else the current scheme is needed. Describes how the
  invocation's transport was authenticated, not how its subject was established.

## [4.2.2] - 2026-08-17

### Updated

- Updated NuGet packages.

## [4.2.1] - 2026-08-16

### Updated

- Updated NuGet packages.

## [4.2.0] - 2026-08-04

### Changed

- **Removed the dormant `AuthorizationDenial` record.** It was never referenced by any package —
  framework or consumer — and the `DenyCodes` summary's claim that codes are emitted "in
  `AuthorizationDenial.Code`" was never true (codes reach telemetry only). `Cirreum.Exceptions`
  is the denial-path carrier and, being a lower layer, could never have referenced this type.
  Deleted under the dormant-surface rule; shipped as a Minor deliberately — with zero consumers,
  the removal cannot break a compile anywhere.
- `DenyCodes`' summary now states where codes are actually emitted: telemetry
  (`cirreum.authz.reason`).

## [4.1.0] - 2026-08-03

### Added

- **`AuthorizationTelemetry.StagePreflight`** — a stage for the checks that run before Stage 1
  evaluates the authorizable object, joining `scope` / `resource` / `policy`. With it, four
  steps: `StepAuthentication`, `StepApplicationUser`, `StepRoles`, `StepAuthorizerPresence`.
  These denials previously reported no stage and never reached `cirreum.authz.decisions`.
- **`DenyCodes.NoRolesAssigned`, `NoAuthorizersRegistered`, `EvaluationError`, and `Unknown`** —
  codes for the denials that had none, retiring the last inline reason strings in the pipeline.
- **The pipeline's telemetry rules are documented on `AuthorizationTelemetry`**: every denial
  records a decision with stage, step, and a `DenyCodes` reason; every terminal outcome records
  duration with decision and reason; a pass records a decision only where a stage actually
  evaluated the object; no reason is ever an inline string.

### Changed

- **`DenyCodes.AuthenticationRequired` is now emitted.** It was declared but unused — the
  unauthenticated path reported the literal `"unauthenticated"` instead. ⚠️ Alongside it,
  `"no-roles"`, `"no-authorizers"`, and `"error"` become `NO_ROLES_ASSIGNED`,
  `NO_AUTHORIZERS_REGISTERED`, and `EVALUATION_ERROR`. **Queries and alerts matching the old
  lowercase reason values will go quiet**, not wrong — they match nothing rather than matching
  the wrong thing.
- `DenyCodes.UserDisabled`'s documentation now states what the code does and does not cover:
  it is emitted for a resolved application user reporting `IsEnabled = false`, and callers with
  no application-user record are not subject to it.

### Updated

- Re-pinned `Cirreum.Kernel` `2.0.1` → `2.0.2` (documentation-only patch rewriting
  `IOwnedApplicationUser` around what `OwnerId` is for).

## [4.0.1] - 2026-07-31

### Updated

- Re-pinned `Cirreum.Kernel` `2.0.0` → `2.0.1` (documentation-only upstream patch:
  `IOwnedApplicationUser.OwnerId` is an identity fact, not an access fact — the doc half of
  this major's records-only grant semantics).

## [4.0.0] - 2026-07-31

### Breaking

- **`IOperationGrantProvider.ResolveHomeOwnerAsync` removed.** Home-company membership access is
  now expressed as a grant record (e.g., a company-self-grant row) like every other owner-scoped
  access — the framework no longer merges an implicit, permission-blind, revoke-blind home owner
  into the granted set. Grant records are the only source of owner-scoped access. ⚠️ **Apps must
  seed home grant rows BEFORE upgrading or tenant users fail closed** — see `MIGRATION-v4.md`
  for the deploy-order walkthrough. Paired with `Cirreum.Domain` 4.0.0, which removes the merge
  from the grant-factory orchestrator.

### Added

- **`PermissionSet.IsSatisfiedBy`** — the canonical grant-entry matcher: AND semantics across
  the required set, case-insensitive exact `feature:operation` matching, bare-action shorthand
  (`"read"` matching any feature) as an explicit opt-in flag, blank/malformed entries never
  match. Replaces the hand-rolled per-app `Satisfies`/`MatchesPermission` helpers whose case and
  wildcard semantics silently drifted. String and parsed-`Permission` overloads.
- **First test suite** (`tests/Cirreum.Contracts.Tests`): 26 tests covering the matcher's exact,
  shorthand, case-variance, and blank/malformed paths.

## [3.0.0] - 2026-07-30

### Breaking

- **`AuthorizationTelemetry.StepPolicyValidator` → `StepPolicyAuthorizer`**, and its emitted wire
  value `policy-validator` → `policy-authorizer`. Part of the framework-wide "policy authorizer"
  vocabulary correction (paired with `Cirreum.Domain` 3.0.0's `IPolicyValidator` →
  `IPolicyAuthorizer` rename): the Stage 3 extension point performs authorization, not
  FluentValidation property validation, and the telemetry now says so. Dashboards and alerts
  filtering on the step value need updating; `StagePolicy` (`"policy"`) is unchanged. See
  `MIGRATION-v3.md`.

### Fixed

- **`IResourceAccessEvaluator` documents its authorization-pipeline dependency**: the evaluator
  reads the caller from the context the operation-authorization pipeline populates, and calling it
  outside an authorized invocation (background services, queue consumers, startup jobs — including
  through surfaces built on it such as protected repositories) throws rather than evaluating
  against a missing caller. Behavior unchanged; the contract now states it.
- Stage 3 documentation language across the authorization contracts now reads "policy
  authorizers" (was "policy validators").

## [2.0.1] - 2026-07-30

### Fixed

- **Removed the orphaned internal `Authorization<,>` intercept** (`AuthorizationIntercept.cs`).
  The type relocated to `Cirreum.Domain`, where the Conductor default pipeline actually registers
  it. It had shipped here unreferenced since the 2.0.0 foundation split — internal, and invoked by
  nothing on any composition path. See the paired `Cirreum.Domain` release for the fail-open
  authorization regression this closes.
- **`IOperationGrantProvider.ResolveHomeOwnerAsync` documentation now states the merge
  semantics explicitly**: a non-null home owner is merged into the granted set with no permission
  check, granting unconditional access on that owner for every operation that reaches grant
  resolution. Returning `null` is the only way to withhold home access — revoking a grant record
  does not revoke it. The behavior is unchanged; the contract now says what it does.
- **`AuthorizationTelemetry.StartActivity` states `ActivityKind.Internal` explicitly** instead of
  taking it as the silent default. `Internal` was already the correct (and effective) kind; the
  choice is now recorded so it reads as deliberate. Mirrors the authentication track's
  transformation-activity change in `Cirreum.Runtime.AuthenticationProvider` 2.0.0. No behavior
  difference. (Folded in from the backlog.)

## [2.0.0] - 2026-07-26

### Updated

- Re-pinned `Cirreum.Kernel` `1.3.0` → `2.0.0`, which carries the renames and the removal this
  release follows.

### Changed

- **Feature state contracts move from the `Cirreum.State` namespace to the root `Cirreum`
  namespace.** `IActivityState`, `INotificationState`, `IThemeState`, `IInitializable`,
  `IInitializableRemoteState`, `IInitializationOrchestrator`, `Notification`, `NotificationType`,
  `ActivityError`, `ActivityErrorSeverity`, and `ActivityMode` are all affected.

  `Cirreum.State` had accumulated two unrelated populations: the state *machinery* an application
  builds on — `IRemoteState`, `IStateBuilder`, `IScopedNotificationState` — and the concrete
  *feature* states it consumes. The second group is what applications reference constantly, and
  putting it behind a namespace import made the common case the inconvenient one. `Cirreum.State`
  now holds the machinery only.

- **`IPageState` → `IBrowserDocumentState`.** The type governs the browser document hosting the
  application — its title, application name, and progressive-web-app display mode — none of which
  are properties of a Blazor *page*. The old name invited exactly that confusion in a framework
  where "page" already means a routable component.

- **`IPublisher.PublishAsync` is constrained to `IDomainEvent`** instead of `INotification`,
  following the rename in `Cirreum.Kernel` 2.0.0. The type parameter is `TDomainEvent` and the
  first argument is `domainEvent`; the constraint and the argument type are the only source-level
  changes, and dispatch semantics are unchanged.

  Cirreum used "notification" for two unrelated things — Conductor's in-application
  publish/subscribe primitives, and the human-facing state family a client binds to in order to
  show a person something. **`INotificationState` and `IScopedNotificationState` keep their
  names**: they are the human-facing concept, and preserving that separation is the point of the
  rename.

### Removed

- **`OperationContext.Provider` / `.IsFromProvider(IdentityProviderType)` and
  `AuthorizationContext.Provider` / `.IsFromProvider(IdentityProviderType)`**, following the
  removal of `IdentityProviderType` from `Cirreum.Kernel`.

  Both were pass-throughs to `UserState.Provider`, a value inferred per request by matching the
  `iss` claim against a built-in table of vendor domains — configuration re-derived by guesswork.
  Neither had a call site in the framework, and `IsFromProvider` had none in any consuming
  application either.

  `AuthorizationContext` exposing it was the sharper problem: it invited an authorizer to gate
  access on a best-effort string match that returns `Unknown` for a valid token whose provider
  uses a custom domain. The authoritative per-request answer to "which identity provider
  authenticated this caller" is the authenticated scheme, which is configuration-tied rather than
  inferred and is what every other per-scheme lookup in the framework already dispatches on.

  **Migration.** For "is this user authenticated?", use `IsAuthenticated` — the check that was
  meant. For "which provider issued this token?", read `UserProfile.Issuer`, which carries the
  `iss` claim verbatim. For a capability that happens to correlate with a provider, prefer the
  capability signal itself; it survives adding a second identity provider where a provider check
  does not. See `MIGRATION-v2.md`.

## [1.4.5] - 2026-07-24

### Updated

- Updated NuGet packages.

## [1.4.4] - 2026-07-20

### Updated

- Updated NuGet packages.

## [1.4.3] - 2026-07-07

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
