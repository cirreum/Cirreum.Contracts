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

### Rename `RemoteConnectionTokenRequest` to `RemoteConnectionCredentialRequest`

**SemVer:** Patch
**Trigger:** the next `Cirreum.Contracts` major — the rename itself is breaking and cannot ship below one.
**Noted:** 2026-08-26

5.0.0 widened the seam from tokens to credentials: `IRemoteConnectionTokenSource` became
`IRemoteConnectionCredentialSource`, `GetAccessTokenAsync` became `GetCredentialAsync`, and the
return type became `AuthorizationHeaderSettings?` so any authorization scheme can be expressed. The
request record did not follow, so the surface reads
`IRemoteConnectionCredentialSource.GetCredentialAsync(RemoteConnectionTokenRequest)` — a credential
source taking a token request.

Cosmetic, and it only gets more expensive: every consumer implementing a source names the type.

**Listed at `Patch` deliberately, so it surfaces on every release rather than only when a Contracts
major is already in flight — that release is rare, and this is exactly the item that gets forgotten
between them. Executing it is a Major**, and no `[Obsolete]` staging: the old name is deleted
outright.

Fold into the typed-client-binding work if that lands as a Contracts-touching release, or into
whatever forces the next major.
