# Cirreum.Contracts 4.6.0 — the connection half of RemoteServices gains its configuration surface

## Why this release exists

`RemoteServices` has carried two halves since the foundation reset. The request/response half
is complete: `RemoteClient` for the implementation, `RemoteServiceOptions` for its
configuration, and host runtimes that register typed clients from those options.

The long-lived half was not. `IRemoteConnection`, `RemoteConnectionState`, and
`RemoteConnectionStateChangedEventArgs` describe a persistent bidirectional channel, but
nothing described how one is *configured*, where its credentials come from, or how a caller
obtains one whose lifetime is shorter than the application's. Every consumer that reached for
the abstraction had to answer those questions itself.

4.6.0 supplies the missing contracts so transport packages can implement the abstraction
without inventing their own configuration vocabulary, and without binding themselves to a
particular host.

## What's new

### `RemoteConnectionOptions`

The long-lived counterpart to `RemoteServiceOptions`, deliberately parallel in shape — same
constructor forms, same `ApplicationName` derivation, same `AuthorizationHeaderSettings`
posture, same value-equality treatment:

```csharp
var options = new RemoteConnectionOptions("MyApp", new Uri("https://api.example.com/hubs/chat")) {
    Reconnect = true,
    ReconnectMaxDelay = TimeSpan.FromSeconds(30)
};
```

Credentials resolve in a fixed order — `AccessTokenProvider` when set, then
`AuthorizationHeader`, then the ambient token source below.

The type implements `IEquatable<RemoteConnectionOptions>`. This is load-bearing rather than
decorative: registration surfaces use it to treat a repeated identical registration as a
no-op and a conflicting one as an error, the same rule the named-`HttpClient` registration
already applies. `AccessTokenProvider` participates by delegate equality, so two separately
declared callbacks are correctly unequal.

### `IRemoteConnectionTokenSource`

```csharp
public interface IRemoteConnectionTokenSource {
    ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
```

The ambient credential seam. A host runtime registers an implementation over whatever
identity source that host has, and a transport package consumes the interface — which is what
lets one connection package serve a browser client and a server-side caller without a
host-specific build.

### `IRemoteConnectionFactory<TConnection>`

```csharp
public interface IRemoteConnectionFactory<TConnection> where TConnection : IRemoteConnection {
    TConnection Create(Action<RemoteConnectionOptions>? configure = null);
}
```

For connections whose lifetime is a session rather than the application — one per active call,
one per bridge. Instances are owned by the caller, which connects and disposes them; the
optional delegate adjusts a copy of the registered options for that instance.

## Also in this release

The remarks on `IRemoteConnection` referred to a package family that was never published, left
behind by an earlier restructure. The text now describes the transport-agnostic contract
without naming packages, and notes that implementations may surface capability their transport
provides beyond the interface.

## Compatibility

* **Purely additive** — three new types, no changes to existing members.
* No implementer or consumer changes are required.
* `RemoteServiceOptions` and the request/response surface are untouched.

## See also

* `RemoteServiceOptions` — the request/response sibling.
* `IRemoteConnection`, `RemoteConnectionState` — the connection contract these options configure.
