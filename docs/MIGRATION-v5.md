# Cirreum.Contracts v4 → v5 Migration

v5 breaks the outbound-connection credential surface, and nothing else. If your application does not
implement `IRemoteConnectionTokenSource` and does not set `RemoteConnectionOptions.AccessTokenProvider`,
upgrading is a version bump.

Both members shipped in 4.6.0. The break is taken now, while the surface is days old, precisely so it
is not taken later against more consumers.

---

## Breaking changes — find/replace

| v4 | v5 |
| --- | --- |
| `IRemoteConnectionTokenSource` | `IRemoteConnectionCredentialSource` |
| `GetAccessTokenAsync(CancellationToken ct)` | `GetCredentialAsync(RemoteConnectionTokenRequest request, CancellationToken ct)` |
| returns `ValueTask<string?>` | returns `ValueTask<AuthorizationHeaderSettings?>` |
| `RemoteConnectionOptions.AccessTokenProvider` | `RemoteConnectionOptions.CredentialProvider` |
| `Func<CancellationToken, ValueTask<string?>>` | `Func<CancellationToken, ValueTask<AuthorizationHeaderSettings?>>` |

### A token source

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
public sealed class ApiTokenSource(IAccessTokenProvider tokens) : IRemoteConnectionCredentialSource {

    public async ValueTask<AuthorizationHeaderSettings?> GetCredentialAsync(
        RemoteConnectionTokenRequest request, CancellationToken ct = default) {

        if (request.Scopes.Count == 0) {
            return null;
        }

        var result = await tokens.RequestAccessToken(new AccessTokenRequestOptions {
            Scopes = request.Scopes,
        });

        return result.TryGetToken(out var token)
            ? new AuthorizationHeaderSettings { Scheme = "Bearer", Value = token.Value }
            : null;
    }

}
```

### A per-connection callback

```csharp
// v4
options.AccessTokenProvider = async ct => await this.GetTokenAsync(ct);

// v5
options.CredentialProvider = async ct =>
    new AuthorizationHeaderSettings { Scheme = "Bearer", Value = await this.GetTokenAsync(ct) };
```

## What the request carries

| Member | Use |
| --- | --- |
| `EndpointUri` | The endpoint being connected to, for a source that resolves an audience from the host. |
| `Scopes` | The scopes the connection's options declare. Empty when the application named none. |
| `ConnectionType` | The connection type, and the key a source may be registered against. |

It is declared with required init-only properties, not positionally, so future members are additive:

```csharp
var request = new RemoteConnectionTokenRequest {
    EndpointUri = new Uri("https://api.contoso.com/hubs/chat"),
    Scopes = ["api://contoso/access_as_user"],
    ConnectionType = typeof(ChatConnection),
};
```

## Three return values, three meanings

`GetCredentialAsync` and `CredentialProvider` share one convention:

| Return | Meaning |
| --- | --- |
| a populated `AuthorizationHeaderSettings` | present this credential |
| `AuthorizationHeaderSettings.None` | connect without a credential, deliberately |
| `null` | no credential is available |

v4's `string?` could not express the middle case from the ambient source. Declining and failing were
the same answer.

## New capabilities

### Scopes on the options

Name the audience per connection, with no source to write:

```csharp
builder.AddRemoteConnection<ChatConnection>(options => {
    options.EndpointUri = new Uri("https://api.contoso.com/hubs/chat");
    options.Scopes = ["api://contoso/access_as_user"];
});
```

`Scopes` participates in equality, so registering one connection type twice with different scopes is
a conflict rather than a silent no-op — the same rule that already applied to the endpoint.

### A source per connection

Register keyed by connection type when one connection needs a different mechanism or identity
provider than another. The keyed registration is preferred; the unkeyed one remains the fallback.

```csharp
services.AddKeyedScoped<IRemoteConnectionCredentialSource, PartnerCredentialSource>(typeof(PartnerConnection));
```

### Non-bearer credentials

The ambient source can now supply any scheme — an ApiKey credential, for instance — where v4 could
only return a bearer token.

## Behavioural change to expect from the host runtime

Paired with `Cirreum.Runtime.Wasm`, a connection that declares **no** scopes no longer receives the
host's default-scope token. The source returns nothing and logs, so the connection fails at connect
with a named cause instead of being refused later by a server that did not recognise the audience.

Under v4 that default produced a Microsoft Graph token, which no first-party API accepts. If a
connection relied on the host defaults, declare them explicitly on its options.

## What didn't change

- The credential resolution order: `CredentialProvider`, then `AuthorizationHeader`, then the ambient
  source. An application already setting `AuthorizationHeader` is unaffected by all of the above.
- `IRemoteConnection`, `IRemoteConnectionFactory<TConnection>`, and every other member of
  `RemoteConnectionOptions`.

## Downstream package impact

| Package | Minimum version |
| --- | --- |
| `Cirreum.Domain` | the release that bumps its Contracts floor to 5.0.0 |
| `Cirreum.RemoteConnections.SignalR` / `.WebSockets` | the release that builds the request and resolves the source |
| `Cirreum.Runtime.Wasm` | the release carrying the scope-aware credential source |
| `Cirreum.Runtime.RemoteConnections.SignalR` / `.WebSockets` | the release that stamps `ConnectionType` |
