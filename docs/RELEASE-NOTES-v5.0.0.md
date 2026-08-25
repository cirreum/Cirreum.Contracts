# Cirreum.Contracts 5.0.0 — a credential source can see what it is supplying for

## Why this release exists

`IRemoteConnectionTokenSource` shipped in 4.6.0 with no parameters. A host registered one, and every
outbound connection in the application got the same answer — nothing in the call distinguished a hub
on the application's own API from a partner service.

Where the host could not infer an audience, it did not fail. It answered with whatever its defaults
produced. For the WebAssembly runtime those defaults are the MSAL host's default scopes, which are
Microsoft Graph scopes, so the framework's out-of-box credential for a connection was a Graph token —
rejected by any first-party API, and rejected *at the server*, so it read as an application
authentication bug rather than a credential aimed at the wrong resource. The first external consumer
hit exactly that and worked the answer out from source.

The interface was four days old. This release fixes it once, and widens the surface far enough that
the foreseeable reasons to break it again are gone.

## What's new

**`RemoteConnectionTokenRequest`** describes the connection a credential is being obtained for —
endpoint, declared scopes, and connection type — with required init-only properties rather than
positional parameters, so future members are additive:

```csharp
public sealed record RemoteConnectionTokenRequest {
    public required Uri EndpointUri { get; init; }
    public required IReadOnlyList<string> Scopes { get; init; }
    public required Type ConnectionType { get; init; }
}
```

**`IRemoteConnectionCredentialSource`** replaces `IRemoteConnectionTokenSource`:

```csharp
ValueTask<AuthorizationHeaderSettings?> GetCredentialAsync(
    RemoteConnectionTokenRequest request, CancellationToken cancellationToken = default);
```

**`RemoteConnectionOptions.CredentialProvider`** replaces `AccessTokenProvider` and returns the same
type, so the per-connection slot and the ambient source express the same thing.

**`RemoteConnectionOptions.Scopes`** lets the application name the audience per connection, so the
common case needs no source at all:

```csharp
builder.AddRemoteConnection<ChatConnection>(options => {
    options.EndpointUri = new Uri("https://api.contoso.com/hubs/chat");
    options.Scopes = ["api://contoso/access_as_user"];
});
```

It participates in equality and hash, so registering one connection type twice with different scopes
is the conflict it is rather than a silent no-op.

See [MIGRATION-v5.md](MIGRATION-v5.md).

## Three decisions worth the break

**Replace, don't overload.** A source implementing only the parameterless method would silently
ignore the scopes it was handed — the failure this release exists to remove. A source that cannot see
the request cannot be right about it.

**Credential, not token.** `string?` means bearer. Cirreum ships ApiKey as a first-party scheme, and
the ambient source we have deliberately not yet written — the server-host one — is exactly where a
non-bearer credential appears. Returning `AuthorizationHeaderSettings?` costs a wrapper at every
bearer call site and removes a second major. The name widened with the type rather than leaving a
`TokenSource` that returns credentials.

It also gives the ambient source a value it could not previously express:
`AuthorizationHeaderSettings.None`, meaning *connect without a credential, deliberately* — distinct
from `null`, meaning none is available. Under `string?` those were the same answer.

**A type, not a name.** `ConnectionType` is the key a source may be registered against, so a host can
supply a different mechanism per connection:

```csharp
services.AddKeyedScoped<IRemoteConnectionCredentialSource, PartnerCredentialSource>(typeof(PartnerConnection));
```

Keying by string would have reintroduced the stringly-typed registration this consumer's feedback
opened by objecting to.

## Compatibility

- **Applications that implement neither member**: a version bump, nothing more.
- **Applications setting `AuthorizationHeader`** on a connection's options: unaffected. That posture
  resolves ahead of both and is unchanged.
- **Applications implementing the source or setting the callback**: mechanical changes, both in the
  migration guide.
- Paired with the host runtimes, a connection declaring no scopes stops receiving a default-scope
  token — it fails at connect with a named cause instead of being refused later by the server.

## See also

- `Cirreum.RemoteConnections.SignalR` / `.WebSockets` — build the request from the connection's
  options, resolve the source keyed by connection type, and apply the credential.
- `Cirreum.Runtime.Wasm` — the scope-aware WebAssembly credential source.
