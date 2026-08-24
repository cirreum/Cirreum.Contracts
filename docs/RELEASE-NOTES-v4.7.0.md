# Cirreum.Contracts 4.7.0 — the connection vocabulary gains an endpoint marker

## Why this release exists

Authentication needs to answer one question about an inbound request: *do this endpoint's
invocations arrive over a long-lived connection?*

It matters because of a browser limitation. A browser cannot set request headers on a
WebSocket upgrade, so a client connecting over WebSockets sends its bearer token as an
`access_token` query parameter instead — the convention SignalR's own clients follow. A server
that reads credentials only from `Authorization` refuses those connections, and SignalR
responds by silently falling back to Server-Sent Events or long polling. The application keeps
working and never uses WebSockets, with nothing reporting the downgrade.

Accepting a query-carried credential is therefore necessary, but only on the endpoints that
have no alternative. On an ordinary route it would mean a token in a URL — logged by proxies,
kept in browser history — for no reason. Scoping the acceptance to connection endpoints is
what makes it safe, and scoping requires the endpoint to be identifiable.

SignalR hubs already are: `MapHub` stamps SignalR's own `HubMetadata`. Other long-lived
transports are not, and the layer that reads credentials cannot reference the layer that maps
them.

## What's new

### `InvocationConnectionMetadata`

Endpoint metadata marking an endpoint as one whose invocations arrive over an
`IInvocationConnection`:

```csharp
app.MapWebSocketHandler<VoiceHandler>("/media")   // framework stamps it

app.Map("/custom-stream", handler)                 // an application's own transport
   .WithMetadata(InvocationConnectionMetadata.Instance);
```

The type states a fact and carries no behaviour. It names no credential policy, because it is
not a credential concept — it says the endpoint produces an `IInvocationConnection`, which
this package already defines as the long-lived case: set for SignalR, raw WebSocket and gRPC
streaming, and null for HTTP, gRPC unary and queue handlers. Authentication draws its own
conclusion from that fact, and a future consumer with a different interest can draw a
different one.

It carries no framework dependency, and lives beside the rest of the connection vocabulary, so
the server spine that stamps it and the authentication schemes that read it both reach it
without either referencing the other.

Stateless instance, exposed as a singleton — the marker's presence is the entire signal.

## Compatibility

* **Purely additive** — one new type, no changes to existing members.
* No implementer or consumer changes are required.
* SignalR hubs need no stamp; they are recognized by SignalR's own metadata.

## See also

* `IInvocationConnection` — the connection this metadata says an endpoint produces.
* `IInvocationContext` — the per-invocation seam whose `Connection` is set for these endpoints.
