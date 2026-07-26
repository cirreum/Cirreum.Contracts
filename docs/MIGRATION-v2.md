# Cirreum.Contracts v1 → v2 Migration

## Why v2

`Cirreum.Kernel` v2 removes `IdentityProviderType` — an enum that documented itself as identifying
which identity provider is *configured*, while the implementation inferred it per request by
matching the `iss` claim against a built-in table of vendor domains. `Cirreum.Contracts` surfaced
that value on its two context types, so it drops those members in the same wave.

`AuthorizationContext` exposing it was the sharper problem. `IsFromProvider` on the authorization
surface invites an authorizer to gate access on a best-effort string match — one that returns
`Unknown` for a perfectly valid token whose provider uses a custom auth domain, which is common
enough that Auth0 and Okta both rewrite the issuer to a vanity domain by default. No authorization
decision should rest on that.

Neither member had a call site in the framework, and `IsFromProvider` had none in any consuming
application either.

## Breaking Changes — Find/Replace Table

| Removed | Replace with |
|---|---|
| `OperationContext.Provider` | `context.Profile.Issuer` |
| `OperationContext.IsFromProvider(IdentityProviderType)` | see below — depends on the question |
| `AuthorizationContext.Provider` | `context.UserState.Profile.Issuer` |
| `AuthorizationContext.IsFromProvider(IdentityProviderType)` | see below — depends on the question |

## Migration Walkthrough

### 1. Authorizers gating on a provider

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

The scheme is the value every other per-scheme lookup in the framework already dispatches on —
`IApplicationUserResolver` selection and audience-provider role mapping both use it. It survives
two-phase auth promotion and is propagated across HTTP, SignalR, and WebSocket connections.

If the intent was policy per identity *source* rather than per scheme, register one authorization
policy per scheme instead of branching inside a single authorizer.

### 2. Handlers reading the provider for diagnostics or display

```csharp
// Before
logger.LogInformation("Operation from {Provider}", context.Provider);

// After
logger.LogInformation("Operation from {Issuer}", context.Profile.Issuer);
```

### 3. Handlers branching on a provider for a capability

Gate on the capability rather than the provider — see `Cirreum.Kernel`'s `MIGRATION-v2.md` §2. A
capability check stays correct when a second identity provider is added.

## What Didn't Change

- Every other `OperationContext` and `AuthorizationContext` member — `UserId`, `UserName`,
  `TenantId`, `IsAuthenticated`, `Profile`, `AuthenticationBoundary`, `EffectiveRoles`,
  `HasActiveTenant()`, `IsInDepartment(...)`
- Authorization evaluation, policies, and the authorizer contract
- Conductor dispatch, validation, and the `Result` pipeline

## Downstream Package Impact

Packages that never referenced `.Provider` or `IsFromProvider` need only a re-pin to
`Cirreum.Contracts` 2.0.0 and `Cirreum.Kernel` 2.0.0. Applications should grep for
`IsFromProvider` and `.Provider` before upgrading; both are compile errors rather than silent
behavior changes.
