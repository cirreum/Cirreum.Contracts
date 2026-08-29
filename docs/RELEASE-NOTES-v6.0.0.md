# Cirreum.Contracts 6.0.0

One break: grant extensions are typed.

```diff
- IReadOnlyDictionary<string, object>? Extensions
+ IReadOnlyDictionary<string, IReadOnlyList<string>>? Extensions
```

on `OperationGrant`, `OperationGrant.ForOwners`, and `OperationGrantResult`.

## Why

A grant is cached by value, and `object` does not survive that. A serializing cache provider
deserializes the grant on every hit while a miss returns the resolver's instance directly, so an
extension value arrived as the application's own collection on the first request of a cache key's
lifetime and as a `JsonElement` on every request after it — varying with cache state and with the
cache provider the host had configured, with nothing in the type to say so. Handlers absorbed that
by type-testing the value, which made an unreadable value indistinguishable from a dimension that
was never granted, and left each call site to decide from a failed cast whether that meant deny or
allow.

An auxiliary dimension names the values allowed for it. A list of strings is the shape that
round-trips unchanged, so that is the shape the contract now states.

A set type was considered and rejected: `IReadOnlySet<string>` cannot be deserialized at all, and
`ISet<string>` deserializes without its comparer — a case-insensitively populated dimension would
silently become case-sensitive across a cache hit. Comparison stays with the caller, where the
comparer is already chosen.

## Migrating

Producers drop a cast; readers delete their shape-normalizing helper. Full steps, including the
one behavioural check worth making, in [MIGRATION-v6](MIGRATION-v6.md).

Applications that never populate extensions are unaffected.
