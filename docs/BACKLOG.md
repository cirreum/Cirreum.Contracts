# Backlog

Deferred work for **Cirreum.Contracts**. Items here are tracked but not yet ready
to ship — either because the cost outweighs the benefit in isolation, or
because they're waiting on a forcing function (a related change, a consumer
upgrade, a coordinated multi-repo rollout).

## How this file works

- Each item is a `###` heading so it can be linked to and parsed.
- Each item declares **`SemVer:`** (`Patch` | `Minor` | `Major` | `Unspecified`),
  **`Trigger:`** (the human-readable condition that will make it ready), and
  **`Noted:`** (the date the item was added).
- The Cirreum DevOps release scripts (`PatchRelease`, `MinorRelease`,
  `MajorRelease`) surface items at-or-below the requested bump level so the
  operator can decide whether to fold them in before tagging.
- Items that ship: move from this file to `docs/CHANGELOG.md` under
  `[Unreleased]`. Items that grow into design discussions: promote to an ADR.

## Queued

### State `ActivityKind` explicitly on the authorization activity

- **SemVer:** Patch
- **Trigger:** The next release of this package for any other reason — it is a one-word change with no behavior difference, not worth a release of its own.
- **Noted:** 2026-07-27

`AuthorizationTelemetry.StartActivity` calls `ActivitySource.StartActivity("Authorize Resource")`
without a kind, taking the `ActivityKind.Internal` default.

`Internal` is the **correct** kind — authorization evaluation neither receives work nor sends it. It
runs in-process, always as a child of the span that already accepted the request, so it is never the
span where work arrives and the host-dependent `DomainContext.EntryPointActivityKind` would be wrong
here. Nothing is mislabeled today.

The issue is that the choice is not recorded. `DomainContext` asks a track to state the kind
explicitly *even when it is Internal*, precisely because the framework does vary it — `Client` for
outbound HTTP, `Producer` for broker publishes, `EntryPointActivityKind` for Conductor dispatch. A
silent default reads as "nobody considered it", and the next person adding a span here has no signal
that Internal was deliberate.

```csharp
// Before
var activity = ActivitySource.StartActivity("Authorize Resource");

// After
var activity = ActivitySource.StartActivity("Authorize Resource", ActivityKind.Internal);
```

`Cirreum.Runtime.AuthenticationProvider` 2.0.0 made the same change to the authentication track's
transformation activity; this is the authorization half. `ProvisioningTrace` in
`Cirreum.IdentityProvider` carries the identical item.
