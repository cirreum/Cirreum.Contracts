# Cirreum.Contracts 6.2.0 — Host invocations

## Why this release exists

`IInvocationContext` is produced at every inbound seam — HTTP middleware, the SignalR hub
filter, the WebSocket frame handler — and read by everything that needs to know who is acting:
user-state assembly, authorizers, audit stamps. Nothing produced one for work the host starts
on its own: a hosted-service loop, a startup task, a received message, or work a request hands
off to the thread pool after its response has gone.

The first consumer to hit this had written the correct thing:

```csharp
using (ExecutionContext.SuppressFlow()) {
    return Task.Run(work);
}
```

The `SuppressFlow` is right. Without it the request's invocation context flows into the
background task through the `AsyncLocal`, and that context is dead once the response has gone:
its service scope is disposed, its `Aborted` token has fired, its `Items` alias a finished
request. What the helper could not do was put a *live* context in its place, so the work ran
as the anonymous caller — every row attributed to `Guest`, every operation authorized as
nobody.

This release adds the missing kind of invocation.

## What's new

### `IHostInvocationFactory`

```csharp
await using var invocation = factory.Begin();
await DoWork(invocation.Context);
```

`Begin` establishes an `IInvocationContext` for host work: its own service scope, cancellation
linked to application shutdown, no connection, and a principal that names the host
application. The contract lives here; the implementation ships in the server package and is
registered by `AddCoreServices`.

`Begin` refuses to run inside a current invocation. A caller that reaches it with a request
context still flowing has forgotten to suppress flow, and the exception says so.

### `StartDetached`

```csharp
await factory.StartDetached(async context => {
    await DoWork(context);
});
```

The one ordering that makes the nesting check unreachable — suppress flow, queue to the
pool, begin — shipped so callers do not hand-roll it. It does nothing else: no catch, no log,
no retry. Those are application decisions.

### `HostInvocationOptions`

| Member | Meaning |
|---|---|
| `Source` | `IInvocationContext.InvocationSource`; defaults to `InvocationSources.Host` |
| `User` | Run as this subject instead of the host. Must be authenticated; never modified |
| `OriginScheme` | How `User` was established; stamped as the origin scheme |
| `Roles` | Roles minted for the work; `null` takes the host's configured default |
| `CancellationToken` | Linked into `Aborted` alongside shutdown |

A request that continues work after its response snapshots `invocation.User` and
`invocation.EffectiveScheme` *before* suppressing flow and passes them as `User` and
`OriginScheme`. The invocation then reads as the host acting for that subject: the
authenticated scheme is `Cirreum.Host`, the origin is the request's, and subject kind, boundary
and application user resolve for the subject rather than for the host.

### Constants

- `HostInvocationDefaults.Scheme` — `Cirreum.Host`, the scheme stamped on every host
  invocation and declared as a machine scheme.
- `InvocationSources.Host` — `host`. `Timer` and `Queue` keep their serverless-trigger
  meaning; a hosted service that uses a timer is still host work.

## How it pairs with the rest of the platform

- **`Cirreum.Services.Server`** implements the factory, mints the host principal, and declares
  `Cirreum.Host` as `SubjectKind.Machine`.
- **`Cirreum.Runtime.Authentication`** resolves the host scheme to the `Global` boundary.
- **`Cirreum.Messaging.Distributed` 2.1.0** gives the envelope a `Subject`, and
  **`Cirreum.Runtime.Messaging`** dispatches received messages inside a host invocation.

## Compatibility

Additive. No existing member changed shape. Consumers that reference 6.1.0 compile unchanged.

## See also

- `docs/CHANGELOG.md` — `[6.2.0]`
