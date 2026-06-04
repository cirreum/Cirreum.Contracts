# Cirreum.Contracts 1.0.0 — Cross-host abstractions for the framework spine

`Cirreum.Contracts` is the spine's abstraction layer: the CQRS Conductor surface, caching, state, presence, remote services, file system, and the Authorization-pillar vocabulary and contracts — the cross-host primitives that the spine packages and provider tracks consume. 1.0.0 establishes them as one cohesive foundation above `Cirreum.Kernel`.

**Strictly additive — initial release.** No predecessor `Cirreum.Contracts` package. Targets .NET 10.0.

---

## Why this release exists

The **Cirreum 1.0 Foundation Reset** splits the framework's cross-host concerns into *contracts* (this package) and *implementations* (`Cirreum.Domain`). Contracts is where they live — the ones the spine and every provider track depend on without taking a dependency on a concrete.

By the track-coupling principle, concerns that don't warrant a dedicated track of their own live here, so they're reachable through the one path every host shares. Concerns that *do* have their own track (Messaging, Storage) stay in their own packages; the Authentication event surface folded down into `Cirreum.Kernel`. What remains is a single, predictable home for "the things the spine needs to agree on."

---

## What's new

### Conductor — the CQRS surface

```csharp
Result<Order> result = await dispatcher.DispatchAsync(new GetOrder(id), ct);
```

`IDispatcher` / `IPublisher`, the operation and intercept contracts (HandlerPerformance, QueryCaching, Validation), the builders, and Conductor telemetry/logging. The Result-free notification *markers* live in Kernel; the dispatch surface lives here. (Concretes — `Dispatcher`, `Publisher`, the pipeline — ship in `Cirreum.Domain`.)

### Caching

```csharp
public interface ICacheService {
    ValueTask<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory,
        CacheExpirationSettings settings, string[]? tags = null, CancellationToken ct = default);
    ValueTask RemoveByTagAsync(string tag, CancellationToken ct = default);
}
```

`ICacheService` (get-or-create + tag-based invalidation) with the `InMemoryCacheService`, `InstrumentedCacheService`, `NoCacheService`, `CacheProvider`, settings, and telemetry. The tag surface is what lets subsystems invalidate by caller/credential/feature rather than by individual key.

### State, Presence, RemoteServices, FileSystem

Cross-host **state** abstractions (`IStateContainer`, `IStateManager`, `IStatePersistence`, …) and the `ScopedNotificationState` shape; **presence** (`IUserPresenceService`, `IUserPresenceMonitor`, `UserPresence`, `PresenceStatus`); **remote services** (`IRemoteConnection`, `RemoteClient`, connection state, telemetry); and the **file system** surface (`IFileSystem`, `IMauiHybridFileSystem`, CSV reader/builder, `SystemIOExtensions`, `PathType`).

### Result extensions

`ResultExtensions` — the FluentValidation → `Result<T>` glue that lets validation flow through the Conductor pipeline as `Result` rather than exceptions.

### The Authorization pillar (vocabulary + contracts)

```csharp
[RequiresGrant("orders:write")]
public sealed record CancelOrder(Guid OrderId) : IOperation<Result>;
```

The pillar's vocabulary (`AuthorizationPolicies`, `ApplicationRoles`, `Role`, `Permission`, `PermissionSet`, `DenyCodes`, `AuthorizationTelemetry`) and its cross-host contracts — `AuthorizationContext`, `IAuthorizationEvaluator`, `IAuthorizer`, `IAuthorizationRoleRegistry`, the operation / grant / resource-ACL abstractions, and `[RequiresGrant]`. The **default implementations and FluentValidation validators ship in `Cirreum.Domain`**; the contracts are here so any host can authorize against the same model.

---

## Why this lives in Cirreum.Contracts

Contracts holds abstractions, not concretes — so a consumer can depend on `IDispatcher` or `ICacheService` without pulling in an implementation, and `Cirreum.Domain` can supply the concrete behind the same contract. It references `Cirreum.Kernel` (and the foundation peers as needed) but, per the no-sibling-references invariant, never its sibling foundation packages — keeping the dependency graph a clean tree.

---

## Coordinated downstream work

`Cirreum.Domain` supplies the implementations of these abstractions; the provider tracks (`Cirreum.{Track}Provider`) extend them; the `Cirreum.Runtime.{Host}` packages compose the two into a working app. Contracts publishes after Kernel and before Domain.

---

## Compatibility

- **Additive.** Initial release.
- **.NET 10.0.**
- References `Cirreum.Kernel` (and `Cirreum.Result` / `Cirreum.Exceptions` as needed); no sibling references.
- **Migration from `Cirreum.Core 5.x`:** install `Cirreum.Contracts`. Namespaces are preserved — `Cirreum.Conductor.*`, `Cirreum.Caching.*`, `Cirreum.RemoteServices.*`, `Cirreum.FileSystem.*`, `Cirreum.State.*`, `Cirreum.Presence.*`, `Cirreum.Authorization.*`.

---

## See also

- `CHANGELOG.md` — condensed change list for `1.0.0`.
