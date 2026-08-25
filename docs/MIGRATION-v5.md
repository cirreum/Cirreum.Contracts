# Cirreum.Contracts v4 → v5 Migration

v5 carries one breaking change: **`IRemoteConnectionTokenSource.GetAccessTokenAsync` now takes a
`RemoteConnectionTokenRequest`.**

Nothing else in the contract surface moves. If your application does not implement
`IRemoteConnectionTokenSource`, upgrading is a version bump and nothing else — the interface shipped
in 4.6.0 and the host runtimes are its only framework implementors.

---

## Breaking change — find/replace

| v4 | v5 |
| --- | --- |
| `ValueTask<string?> GetAccessTokenAsync(CancellationToken ct = default)` | `ValueTask<string?> GetAccessTokenAsync(RemoteConnectionTokenRequest request, CancellationToken ct = default)` |

```csharp
// v4
public sealed class ApiTokenSource(IAccessTokenProvider tokens) : IRemoteConnectionTokenSource {

    public async ValueTask<string?> GetAccessTokenAsync(CancellationToken ct = default) {
        var result = await tokens.RequestAccessToken(new AccessTokenRequestOptions {
            Scopes = ["api://contoso/access_as_user"],   // hard-coded: the source cannot be told
        });
        return result.TryGetToken(out var token) ? token.Value : null;
    }

}

// v5
public sealed class ApiTokenSource(IAccessTokenProvider tokens) : IRemoteConnectionTokenSource {

    public async ValueTask<string?> GetAccessTokenAsync(
        RemoteConnectionTokenRequest request, CancellationToken ct = default) {

        if (request.Scopes.Count == 0) {
            return null;
        }

        var result = await tokens.RequestAccessToken(new AccessTokenRequestOptions {
            Scopes = request.Scopes,
        });
        return result.TryGetToken(out var token) ? token.Value : null;
    }

}
```

The request carries three things:

| Member | Use |
| --- | --- |
| `Scopes` | The scopes the connection's options declare. Empty when the application named none. |
| `EndpointUri` | The endpoint being connected to, for a source that resolves an audience from the host. |
| `ConnectionName` | The connection type's name. Has no framework reader in this release; present so that resolving a token source per connection later does not break this contract again. |

## Why the signature changed rather than gained an overload

A parameterless source cannot know which connection it is minting for, so one registration answered
identically for every connection in the application. Where the host could not infer an audience it
answered with whatever its defaults produced — which for the WASM runtime meant a Microsoft Graph
token, rejected by any first-party API, with the rejection surfacing as an application
authentication problem rather than a misconfigured credential.

An overload kept for compatibility would have left that shape reachable, and a source implementing
only the old one would silently ignore scopes it was given.

## New capability — scopes on the options

`RemoteConnectionOptions.Scopes` lets an application name the audience per connection, with no token
source to write:

```csharp
builder.AddRemoteConnection<ChatConnection>(options => {
    options.EndpointUri = new Uri("https://api.contoso.com/hubs/chat");
    options.Scopes = ["api://contoso/access_as_user"];
});
```

`Scopes` participates in equality, so registering one connection type twice with different scopes is
a conflict rather than a silent no-op — the same rule that already applied to the endpoint.

## Behavioural change to expect from the host runtime

Paired with `Cirreum.Runtime.Wasm`, a connection that declares **no** scopes no longer receives the
host's default-scope token. The source returns nothing and logs, so the connection fails at connect
with a named cause instead of being refused later by a server that did not recognise the audience.

If a connection relied on the host defaults, declare them explicitly on its options.

## What didn't change

- The credential resolution order: `AccessTokenProvider`, then `AuthorizationHeader`, then the
  ambient `IRemoteConnectionTokenSource`. An application already supplying either of the first two
  is unaffected by everything above.
- `IRemoteConnection`, `RemoteConnectionOptions` (other than the added `Scopes`), and
  `IRemoteConnectionFactory<TConnection>`.

## Downstream package impact

| Package | Minimum version |
| --- | --- |
| `Cirreum.Domain` | the release that bumps its Contracts floor to 5.0.0 |
| `Cirreum.RemoteConnections.SignalR` / `.WebSockets` | the release that builds the request from options |
| `Cirreum.Runtime.Wasm` | the release carrying the scope-aware token source |
| `Cirreum.Runtime.RemoteConnections.SignalR` / `.WebSockets` | the release that stamps `ConnectionName` |
