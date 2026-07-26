# Cirreum.Contracts v1 → v2 Migration

v2 carries four breaking changes. Two are local to this package — the feature-state namespace
flattening and the `IPageState` rename. Two follow `Cirreum.Kernel` 2.0.0 — the Conductor marker
rename and the removal of `IdentityProviderType`.

Every one is a compile error rather than a silent behavior change.

---

## 1. Feature state contracts move to the root namespace

`Cirreum.State` had accumulated two unrelated populations: the state **machinery** an application
builds on, and the concrete **feature states** it consumes. The second group is what applications
reference constantly, and putting it behind a namespace import made the common case the
inconvenient one.

**Moved to `Cirreum`:**

| | |
|---|---|
| `IActivityState` | `INotificationState` |
| `IThemeState` | `IInitializable` |
| `IInitializableRemoteState` | `IInitializationOrchestrator` |
| `Notification` | `NotificationType` |
| `ActivityError` | `ActivityErrorSeverity` |
| `ActivityMode` | |

**Stays in `Cirreum.State`** — the machinery: `IRemoteState`, `IStateBuilder`,
`IScopedNotificationState`.

### Migration

Usually a deletion rather than an edit. `Cirreum` is almost always already imported — via a global
using, or because the file uses `UserProfile`, `Result`, or any other root type — so the fix is to
drop the now-unused import:

```csharp
using Cirreum.State;   // ← often removable
```

Keep it in files that touch the machinery (`IRemoteState`, `IStateBuilder`,
`IScopedNotificationState`). A file that uses both needs both, which is the point of the split.

---

## 2. `IPageState` → `IBrowserDocumentState`

| Before | After |
|---|---|
| `IPageState` | `IBrowserDocumentState` |

The type governs the browser document hosting the application — its title, application name, and
progressive-web-app display mode. None of those are properties of a Blazor *page*, and in a
framework where "page" already means a routable component, the old name invited exactly that
confusion.

A rename only; every member is unchanged. Note it also moved to the `Cirreum` namespace per §1.

---

## 3. `IPublisher.PublishAsync` is constrained to `IDomainEvent`

Follows the marker rename in `Cirreum.Kernel` 2.0.0.

| Before | After |
|---|---|
| `PublishAsync<TNotification>(TNotification notification, …)` | `PublishAsync<TDomainEvent>(TDomainEvent domainEvent, …)` |
| `where TNotification : INotification` | `where TDomainEvent : IDomainEvent` |

Call sites are unchanged — `await publisher.PublishAsync(new OrderPlaced(id))` compiles as-is once
the event type implements `IDomainEvent`. Only types that *name* the type parameter, or declare the
constraint themselves, need editing. Dispatch semantics, fan-out, and publishing strategies are
untouched.

**`INotificationState` and `IScopedNotificationState` keep their names.** They are the human-facing
concept — what a client binds to in order to show a person something — and separating them from
in-application publish/subscribe is the entire purpose of the rename. **A project-wide find/replace
of "Notification" will undo it.**

---

## 4. `Provider` and `IsFromProvider` are removed from both contexts

Follows the removal of `IdentityProviderType` from `Cirreum.Kernel` 2.0.0.

| Removed | Replace with |
|---|---|
| `OperationContext.Provider` | `context.Profile.Issuer` |
| `OperationContext.IsFromProvider(…)` | depends on the question — see below |
| `AuthorizationContext.Provider` | `context.UserState.Profile.Issuer` |
| `AuthorizationContext.IsFromProvider(…)` | depends on the question — see below |

Both were pass-throughs to a value inferred per request by matching the `iss` claim against a
built-in table of vendor domains. `AuthorizationContext` exposing it was the sharper problem: it
invited an authorizer to gate access on a best-effort string match that returns `Unknown` for a
valid token whose provider uses a custom domain — and custom auth domains are common enough that
Auth0 and Okta both rewrite the issuer to a vanity domain by default.

### 4a. Authorizers gating on a provider

Don't reintroduce the check — replace it with the authoritative signal:

```csharp
// Before
if (!context.IsFromProvider(IdentityProviderType.Entra)) {
	return AuthorizationResult.Forbidden();
}

// After — the authenticated scheme is configuration-tied, not inferred
var scheme = invocation.Items[AuthenticationContextKeys.AuthenticatedScheme] as string;
if (!string.Equals(scheme, "entraWorkforce", StringComparison.Ordinal)) {
	return AuthorizationResult.Forbidden();
}
```

The scheme is what every other per-scheme lookup in the framework already dispatches on —
`IApplicationUserResolver` selection and audience-provider role mapping both use it. It survives
two-phase auth promotion and is propagated across HTTP, SignalR, and WebSocket connections.

If the intent was policy per identity *source* rather than per scheme, register one authorization
policy per scheme instead of branching inside a single authorizer.

### 4b. Handlers reading the provider for diagnostics or display

```csharp
// Before
logger.LogInformation("Operation from {Provider}", context.Provider);

// After
logger.LogInformation("Operation from {Issuer}", context.Profile.Issuer);
```

### 4c. Handlers branching on a provider for a capability

Gate on the capability rather than the provider — see `Cirreum.Kernel`'s `MIGRATION-v2.md` §2. A
capability check stays correct when a second identity provider is added; a provider check does not.

---

## What Didn't Change

- Every other `OperationContext` and `AuthorizationContext` member — `UserId`, `UserName`,
  `TenantId`, `IsAuthenticated`, `Profile`, `AuthenticationBoundary`, `EffectiveRoles`,
  `HasActiveTenant()`, `IsInDepartment(...)`
- Every member of every moved or renamed state contract
- Authorization evaluation, policies, and the authorizer contract
- Conductor dispatch, validation, and the `Result` pipeline

## Downstream Package Impact

Packages that referenced none of the above need only a re-pin to `Cirreum.Contracts` 2.0.0 and
`Cirreum.Kernel` 2.0.0. Before upgrading, grep for `IsFromProvider`, `.Provider`, `IPageState`, and
`using Cirreum.State;` — the first three are compile errors, and the fourth is usually just a
removable import.
