# Cirreum.Contracts 4.3.0 — the invocation answers how it was authenticated

## Why this release exists

Every session-handoff flow needs one fact about the current request: which authentication
scheme authenticated it. A negotiate endpoint minting a session ticket records that scheme as
the ticket's origin, so the subject's declaration keeps resolving from the scheme that actually
authenticated them after the handoff. Until now the only way to read it was the raw slot:

```csharp
var scheme = ctx.Items[AuthenticationContextKeys.AuthenticatedScheme] as string;
```

A string-keyed bag, a cast, and a Kernel constant — an incantation rather than an API, and one
that only worked where the code happened to hold an `HttpContext`. The fact itself is not
HTTP-shaped: SignalR and WebSocket invocations carry the same stamp, seeded from their
connection's authentication state at establishment.

The invocation context is the surface that already answers per-invocation questions uniformly
across sources, so the read now lives there.

## What's new

**`IInvocationContext.AuthenticatedScheme`** — an extension member beside the connection
ownership surface, read through the ambient accessor:

```csharp
app.MapPost("/negotiate", async (
    HttpContext ctx,
    IInvocationContextAccessor invocation,
    ISessionTicketIssuer issuer) => {

    var ticket = await issuer.IssueAsync(new SessionTicketIssueRequest {
        Subject = ClaimsHelper.ResolveId(ctx.User)!,
        Scheme = invocation.Current?.AuthenticatedScheme,
        Lifetime = TimeSpan.FromMinutes(2)
    }, ctx.RequestAborted);

    return Results.Ok(new { ticket = ticket.TicketValue });
}).RequireAuthorization();
```

The semantics are deliberately narrow. It returns the stamped value or `null` — no fallback to
`Identity.AuthenticationType`, which is not a scheme name — so `null` truthfully means the
invocation did not flow through Cirreum scheme dispatch. For long-lived sources the value
describes how the *connection* was authenticated at establishment; it does not change when the
connection is later promoted, and therefore does not necessarily describe how the current
subject was established. That distinction — transport fact versus subject fact — is the origin
model's, and this member is the transport half.

## Removed: `IRequestOrigin`

Defined at the foundation reset as the replacement for the deleted `IUserState.Actor` surface,
and never implemented, produced, or consumed — its own documentation promised an
`IUserState.Origin` that never existed and could not have compiled from Kernel.

Rejected rather than deferred, because each member already has a better home: the invocation
source lives on `IInvocationContext`, application-defined channel facts travel as principal
claims (a session ticket's annotations are readable by a custom principal binder), and
correlation references belong to telemetry. And "origin" now carries a precise meaning in the
authentication model — the scheme that established a subject — that this type would have
muddied.

## Compatibility

- **`AuthenticatedScheme` is additive** — an extension member; no implementer of
  `IInvocationContext` changes.
- **The `IRequestOrigin` removal is breaking by the letter of SemVer and a no-op in practice**:
  zero implementations, producers, or consumers existed framework-wide, verified before
  removal. Shipped in a Minor deliberately on that basis.
- Nothing else changed.

## See also

- `Cirreum.Kernel 2.1.1` — `AuthenticationContextKeys.OriginScheme`, the subject-fact
  counterpart to this release's transport-fact read.
- `Cirreum.Authentication.SessionTicket` — the negotiate samples that motivated the member;
  tickets carry the scheme this member reads.
- `Cirreum.AuthenticationProvider 3.0.1` — the registration funnel whose declarations the
  origin model resolves against.
