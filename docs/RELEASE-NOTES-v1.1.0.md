# Cirreum.Contracts 1.1.0

## Summary

A minor release that re-pins `Cirreum.Contracts` to `Cirreum.Result` `2.0.0`.
Contracts' own contract surface is unchanged — this release exists to rebuild the
package against the corrected `Result` so the fix flows to everything downstream.

## Why

`Cirreum.Result` 2.0.0 fixed a production-only bug: `Result`/`Result<T>` are
`readonly struct`s that System.Text.Json could not round-trip, so a serialized
success came back as a failure. The published `Cirreum.Contracts` 1.0.0 was built
against the pre-fix `Result` 1.0.16, so any consumer caching or transporting a
`Result` through Contracts inherited the bug. Re-pinning and re-releasing
propagates the fix transitively.

## What changed

- **`Cirreum.Result` `1.0.16` → `2.0.0`.** Brings in:
  - The canonical `Result`/`Result<T>` System.Text.Json converter (correct
    round-trip under any `JsonSerializerOptions`).
  - The `IErrorState` opt-in contract and `SurrogateResultException` carrier for
    lossless-enough failure round-trips, plus the `HasError` matchers.
  - The rewritten pagination types (`SliceResult`/`CursorResult`/`PagedResult`).

## Compatibility

- **Minor release.** Contracts' own public API is unchanged.
- Contracts re-exposes the `Cirreum.Result` pagination types transitively. Those
  types were source-rewritten in `Result` 2.0.0 (positional records →
  explicit-ctor records: `Deconstruct`/`with` removed, ctor params renamed).
  A Contracts consumer that deconstructs or `with`-copies those types via Contracts
  must follow the `Cirreum.Result` 2.0.0 migration guide. Consumers that don't use
  pagination are unaffected.
- Multi-target unchanged.

## Upgrade

Update the package reference to `1.1.0`. No Contracts-specific code changes
required; review `Cirreum.Result` 2.0.0's migration notes only if you use the
pagination types.
