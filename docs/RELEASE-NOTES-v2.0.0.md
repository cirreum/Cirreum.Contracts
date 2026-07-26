# Cirreum.Contracts 2.0.0 — Names That Say What They Are

## Why this release exists

Two of the four breaking changes here originate in `Cirreum.Kernel` 2.0.0 and follow it
mechanically. The other two are this package's own, and they are the ones worth reading about,
because nothing upstream explains them.

Both are about a name or a namespace that had stopped describing its contents.

## The state namespace was doing two jobs

`Cirreum.State` had accumulated two unrelated populations:

- The **machinery** an application builds on — `IRemoteState`, `IStateBuilder`,
  `IScopedNotificationState`
- The concrete **feature states** it consumes — `IThemeState`, `INotificationState`,
  `IActivityState`, and the rest

The second group is what applications reference constantly, and putting it behind a namespace
import made the common case the inconvenient one. Eleven feature contracts move to the root
`Cirreum` namespace; the machinery stays in `Cirreum.State`.

In practice this is usually a *deletion* rather than an edit. `Cirreum` is nearly always already
imported, so the fix is removing a now-unused `using Cirreum.State;` — kept only in files that
genuinely touch the machinery.

## `IPageState` was never about a page

Renamed to **`IBrowserDocumentState`**. It governs the browser document hosting the application —
title, application name, progressive-web-app display mode. None of those are properties of a Blazor
page, and in a framework where "page" already means a routable component, the old name invited
precisely that confusion. Members are unchanged.

## Following Kernel

**`IPublisher.PublishAsync` is constrained to `IDomainEvent`.** Cirreum had used "notification" for
two opposite concepts — in-application publish/subscribe, and the human-facing state a client binds
to in order to show a person something. Call sites don't change; only declarations that name the
type parameter do.

Note the deliberate asymmetry: **`INotificationState` and `IScopedNotificationState` keep their
names.** They are the human-facing concept, and separating the two is the whole point. A
project-wide find/replace of "Notification" undoes it.

**`Provider` and `IsFromProvider` are removed from `OperationContext` and `AuthorizationContext`.**
Both were pass-throughs to a value inferred per request by matching the `iss` claim against a
built-in table of vendor domains.

`AuthorizationContext` exposing it was the sharper problem. It invited an authorizer to gate access
on a best-effort string match — one that returns `Unknown` for a perfectly valid token whose
provider uses a custom auth domain, which Auth0 and Okta both do by default. Nothing in the
framework ever called it, and neither did the one application integrating against it.

For "which identity provider authenticated this caller," the authoritative answer is the
authenticated scheme: configuration-tied rather than inferred, and already what every per-scheme
lookup in the framework dispatches on. For "what did the token assert," it is the new
`UserProfile.Issuer`.

## Compatibility

Breaking, and every change is a compile error rather than a silent behavior change — nothing here
alters runtime behavior for code that still compiles.

See [`MIGRATION-v2.md`](MIGRATION-v2.md) for the full find/replace tables and the three questions
hiding behind a provider comparison.

Before upgrading, grep for `IsFromProvider`, `.Provider`, `IPageState`, and `using Cirreum.State;`.
The first three are errors; the fourth is usually just a removable import.

## Coordinated downstream work

Part of the `Cirreum.Kernel` 2.0.0 wave. `Cirreum.Messaging.Distributed`,
`Cirreum.AuthenticationProvider`, `Cirreum.Domain`, `Cirreum.Runtime.AuthenticationProvider`, and
`Cirreum.Runtime.Messaging` each take a major alongside it, with `Cirreum.Authentication.External`
and `Cirreum.Services.Wasm` taking patches. Each ships its own migration guide.

## See also

- [`MIGRATION-v2.md`](MIGRATION-v2.md)
- [`CHANGELOG.md`](CHANGELOG.md)
- `Cirreum.Kernel` 2.0.0 release notes — the origin of changes 3 and 4
