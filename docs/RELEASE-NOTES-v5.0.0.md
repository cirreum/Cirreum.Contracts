# Cirreum.Contracts 5.0.0 — a token source can see what it is minting for

## Why this release exists

`IRemoteConnectionTokenSource` shipped in 4.6.0 with no parameters. A host registered one, and every
outbound connection in the application got the same answer from it — there was nothing in the call to
distinguish a hub on the application's own API from a partner service.

Where the host could not infer an audience, it did not fail. It answered with whatever its defaults
produced. For the WebAssembly runtime those defaults are the MSAL host's default scopes, which are
Microsoft Graph scopes, so the framework's out-of-box credential for a connection was a Graph token —
rejected by any first-party API, and rejected *at the server*, so it read as an application
authentication bug rather than a credential aimed at the wrong resource.

The first external consumer hit exactly that, and worked the answer out from source.

## What's new

**`RemoteConnectionTokenRequest`** describes the connection a token is being obtained for:

```csharp
public sealed record RemoteConnectionTokenRequest(
    Uri EndpointUri,
    IReadOnlyList<string> Scopes,
    string ConnectionName);
```

**`IRemoteConnectionTokenSource.GetAccessTokenAsync` takes it**, replacing the parameterless
signature rather than overloading it. See [MIGRATION-v5.md](MIGRATION-v5.md).

**`RemoteConnectionOptions.Scopes`** lets the application name the audience per connection, so the
common case needs no token source at all:

```csharp
builder.AddRemoteConnection<ChatConnection>(options => {
    options.EndpointUri = new Uri("https://api.contoso.com/hubs/chat");
    options.Scopes = ["api://contoso/access_as_user"];
});
```

It participates in equality and hash, so registering one connection type twice with different scopes
is the conflict it is rather than a silent no-op.

## Why replace rather than overload

An overload retained for compatibility leaves the shape that caused this reachable: a source
implementing only the parameterless method silently ignores the scopes it was handed, which is the
failure the release exists to remove. A source that cannot see the request cannot be right about it.

The interface is four days old and has one known external implementation — written because the
default did not work. The break costs less now than it ever will again.

## What `ConnectionName` is for

Nothing reads it in this release, which is a deliberate exception to the rule against shipping
surface no consumer has.

The token source is resolved inside the transport packages, which never see the connection's generic
type — it stops at the registration layer above them. If resolving a *different source* per
connection is ever wanted, the key has to travel with the request, and adding it then would break
this contract a second time. The field is inert; its absence would be expensive.

## Compatibility

- **Applications that do not implement `IRemoteConnectionTokenSource`**: a version bump, nothing
  more.
- **Applications supplying `AccessTokenProvider` or `AuthorizationHeader`** on a connection's
  options: unaffected. Those postures resolve ahead of the ambient source and are unchanged.
- **Applications implementing the interface**: one signature change, mechanical.
- Paired with the host runtimes, a connection declaring no scopes stops receiving a default-scope
  token — it fails at connect with a named cause instead of being refused later by the server.

## See also

- `Cirreum.RemoteConnections.SignalR` / `.WebSockets` — build the request from the connection's
  options and pass it to the source.
- `Cirreum.Runtime.Wasm` — the scope-aware WebAssembly token source.
