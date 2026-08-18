# Cirreum.Contracts 4.4.0 — the invocation answers which scheme governs its subject

## Why this release exists

4.3.0 shipped the transport half of the origin model: `AuthenticatedScheme`, how the current
invocation's transport was authenticated. The subject half was still an incantation. A session
ticket carries the scheme that established its subject, the ticket handler stamps it, a
promotion will stamp it — and every consumer that cares had to read the raw slot and coalesce
by hand:

```csharp
var origin = ctx.Items[AuthenticationContextKeys.OriginScheme] as string;
var effective = origin ?? authenticatedScheme;
```

That coalesce is the load-bearing line. Subject-kind resolution, application-user resolver
dispatch, and boundary resolution all need *the scheme whose declarations govern the current
subject* — and each call site writing its own `origin ?? scheme` is how one of them eventually
drifts. The rule now exists once, as API.

## What's new

Two extension members beside `AuthenticatedScheme`, completing a triple that mirrors the
connection-ownership surface:

| Connection fact | Scheme fact | Semantics |
|---|---|---|
| `User` | `AuthenticatedScheme` | the transport fact, constant for the connection's life |
| `PromotedUser` | `OriginScheme` | the override fact — `null` unless the subject came from elsewhere |
| `EffectiveUser` | `EffectiveScheme` | what consumers read |

**`OriginScheme`** returns the scheme that established the current subject when it differs
from `AuthenticatedScheme` — carried by a session-ticket continuation or stamped at Two-Phase
Auth promotion — and `null` when the authenticated scheme established the subject itself.

**`EffectiveScheme`** is `OriginScheme ?? AuthenticatedScheme`: the scheme whose declarations
govern the current subject. Negotiate endpoints are its first consumer — recording the
*effective* scheme as a ticket's origin preserves the root through any depth of ticket-refresh
chaining, where recording the authenticated scheme would capture the continuation itself:

```csharp
var ticket = await issuer.IssueAsync(new SessionTicketIssueRequest {
    Subject = ClaimsHelper.ResolveId(ctx.User)!,
    Scheme = invocation.Current?.EffectiveScheme,
    Lifetime = TimeSpan.FromMinutes(2)
}, ctx.RequestAborted);
```

The semantics stay deliberately narrow. All three members read the invocation's own items —
connection slots reach an invocation only through the per-invocation seed, so every subject
fact within one invocation comes from the same snapshot. A promotion during the current
invocation becomes visible beginning with the next invocation, together with the promoted
`User` it describes: the scheme facts never run ahead of, or lag behind, the subject they
qualify. And as with `AuthenticatedScheme`, there is no fallback to
`Identity.AuthenticationType` — `null` truthfully means no stamp exists.

## Compatibility

- **Purely additive** — two extension members; no implementer of `IInvocationContext` changes,
  no existing member's behavior changes.
- `AuthenticatedScheme` reads through a shared private helper now; observable behavior is
  identical.

## See also

- `Cirreum.Kernel 2.1.1` — `AuthenticationContextKeys.OriginScheme`, the slot these members
  read.
- `Cirreum.Authentication.SessionTicket 1.1.0` — stamps the origin at ticket validation;
  its negotiate samples are `EffectiveScheme`'s first call sites.
- `Cirreum.Contracts 4.3.0` — the transport half of this read surface, and the
  `IRequestOrigin` removal that freed the word "origin" for this model.
