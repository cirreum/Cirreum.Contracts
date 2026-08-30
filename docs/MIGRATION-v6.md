# Cirreum.Contracts v5 → v6 Migration

v6 breaks one thing: the value type of grant extensions.

| v5 | v6 |
| --- | --- |
| `IReadOnlyDictionary<string, object>?` | `IReadOnlyDictionary<string, IReadOnlyList<string>>?` |

Affected members: `OperationGrant.Extensions`, the `extensions` argument of
`OperationGrant.ForOwners`, and `OperationGrantResult.Extensions`. An application that never
populates extensions is unaffected and needs no change.

---

## Producing extensions

Drop the cast to `object`. The dictionary the provider already builds is the dictionary the
contract now takes.

```csharp
// v5
return new OperationGrantResult(
    ownerIds,
    merged.ToDictionary(kv => kv.Key, kv => (object)(IReadOnlyList<string>)[.. kv.Value]));

// v6
return new OperationGrantResult(
    ownerIds,
    merged.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<string>)[.. kv.Value]));
```

A dimension whose values were accumulated in a `HashSet<string>` still materializes to a list at
the boundary. The set's comparer never crossed the contract — it did not survive caching in v5
either — so nothing is lost by materializing explicitly.

## Reading extensions

Any helper that normalized the value's runtime shape can be deleted. A lookup is a lookup.

```csharp
// v5 — one shape on a cache miss, another on a hit
private static IReadOnlyList<string>? GetExtension(
    IReadOnlyDictionary<string, object>? extensions, string key) => extensions switch {
        null => null,
        _ when extensions.TryGetValue(key, out var raw) => raw switch {
            IReadOnlyList<string> typed => typed,
            JsonElement json => ReadFrom(json),
            _ => null
        },
        _ => null
    };

// v6
var allowed = grant.Extensions?.GetValueOrDefault(key);
```

**Check what your call sites concluded from a `null`.** In v5 a helper of this shape returned
`null` both for "this dimension was never granted" and for "the value came back in a shape I could
not read". Those are now distinct: `null` means only the former. Where a call site read the merged
`null` as *unrestricted*, it was widening a caller's scope on every request served from a
serializing cache; where it read it as *deny*, it was returning empty results. Both call sites keep
compiling — re-read them and confirm the surviving branch is the one you meant.

## Comparison semantics

The dictionary is still keyed with the comparer you construct it with, and values are compared by
whatever the reading handler chooses:

```csharp
allowed.Contains(candidate, StringComparer.OrdinalIgnoreCase)
```

This is unchanged from v5 in practice. It is called out because a set-typed value would have looked
like it carried its own matching rule while losing the comparer across a cache hit; the list type
makes no such promise.

## What is not changing

`OwnerIds` and the three distinguished shapes — denied, unrestricted, bounded — are untouched, as
are `Contains`, `ContainsAll`, `IsDenied`, `IsUnrestricted`, and `IsBounded`. Extensions remain
opaque to Core: no key or value is interpreted, and no meaning is assigned to an absent dimension.

---

## `RemoteConnectionTokenRequest` → `RemoteConnectionCredentialRequest`

A rename, unrelated to grant extensions, folded in because it could only ship on a major.

| v5 | v6 |
| --- | --- |
| `RemoteConnectionTokenRequest` | `RemoteConnectionCredentialRequest` |

v5 widened this seam from tokens to credentials — `IRemoteConnectionTokenSource` became
`IRemoteConnectionCredentialSource`, `GetAccessTokenAsync` became `GetCredentialAsync`, and the
return type became `AuthorizationHeaderSettings?` so any authorization scheme can be expressed. The
request record did not follow, so the surface read as a credential source taking a token request.

The type's members are unchanged — `EndpointUri`, `Scopes`, `ConnectionType`, all `required
init`-only. Only the name moved:

```csharp
// v5
public ValueTask<AuthorizationHeaderSettings?> GetCredentialAsync(
    RemoteConnectionTokenRequest request, CancellationToken ct = default) { ... }

// v6
public ValueTask<AuthorizationHeaderSettings?> GetCredentialAsync(
    RemoteConnectionCredentialRequest request, CancellationToken ct = default) { ... }
```

A find-and-replace on the type name is the whole migration. It affects any application implementing
`IRemoteConnectionCredentialSource`; an application that only consumes connections never names the
type.
