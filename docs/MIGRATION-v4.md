# Cirreum.Contracts v3 → v4 Migration

v4 carries one breaking change: **`IOperationGrantProvider.ResolveHomeOwnerAsync` is removed.**
Home-company membership access is now expressed as a grant record like every other owner-scoped
access — the framework no longer merges an implicit home owner into the granted set (paired with
`Cirreum.Domain` 4.0.0, which removes the merge from the grant-factory orchestrator).

After this release, grant records are the **only** source of owner-scoped access: if it isn't a
record, it isn't granted.

---

## ⚠️ Seed home grant rows BEFORE upgrading

**This is a behavioral breaking change, not just a compile-time one.** Under v3, every caller
whose `ApplicationUser` implements `IOwnedApplicationUser` received unconditional access to
their home owner — with no grant record, no permission check, and no way to revoke it by
deleting records. Under v4 that implicit access is gone. **A deployment that upgrades without
first seeding home grant rows will fail closed: tenant users lose all home-company access the
moment the new packages go live.**

Deploy in this order:

1. **Seed the rows first** (safe under v3 — the rows simply coexist with the merge):
   - **Company-partitioned grant stores** (recommended): write one *company-self-grant* row per
     company — grantee = the company, owner = the same company, permissions = whatever
     membership should confer:

     ```json
     {
       "granteeType": "Company",
       "granteeId": "<companyId>",
       "ownerId": "<companyId>",
       "permissions": ["loans:read", "loans:write", "documents:read"]
     }
     ```

     Your provider must union company-grantee rows for the caller's home company with the
     caller's user-level rows (`granteeId IN (userId, homeCompanyId)`).
   - **User-partitioned grant stores**: seed one home row per user (owner = the user's home
     company) at provisioning, and backfill existing users.
2. **Verify** membership access works with the merge still present (it will — the merged owner
   is now also granted by record, and the union is idempotent).
3. **Upgrade** to Contracts 4.0.0 / Domain 4.0.0. The implicit merge disappears; the rows carry
   the access.

## 1. `IOperationGrantProvider.ResolveHomeOwnerAsync` — removed

| | Before | After |
|---|---|---|
| Interface member | `ResolveHomeOwnerAsync` (optional hook, default: `IOwnedApplicationUser.OwnerId`) | *(removed)* |
| Home membership access | Implicit — merged with no permission check | A grant record, evaluated like any other |
| Revoking home access | Impossible via records (only returning `null` from the hook) | Delete or narrow the row |

### Migration

- **Providers that never overrode the hook**: no code change — but you were relying on the
  implicit merge, so the seeding step above is mandatory.
- **Providers that overrode it to return `null`** (strict grants-only policy): delete the
  override — v4's behavior is your policy, framework-wide.
- **Providers that overrode it with policy** (suspension checks, membership validation): move
  that policy into the data — express valid membership as rows and revocation as their absence.
  `ResolveGrantsAsync` remains a pure data lookup.

### Why

The hook was permission-blind (bypassed `[RequiresGrant]` matching entirely), revoke-blind
(deleting every grant record for a caller left home access intact), and invisible to the grant
store (auditing the container did not show who had home access). Production providers used it
for zero policy — one restated the default verbatim, the other inherited it. Access is now
uniformly: *a record, or nothing.*

## New capabilities

### `PermissionSet.IsSatisfiedBy` — the canonical grant-entry matcher

Grant providers previously hand-rolled the "does the caller's grant row satisfy the operation's
required permissions?" comparison — with per-app drift in case-sensitivity and wildcard
semantics. `PermissionSet` (the type of `AuthorizationContext.RequiredGrants`) now carries the
canonical matcher:

```csharp
// entries: raw strings off a grant row, e.g. ["loans:read", "loans:write"]
var satisfied = context.RequiredGrants.IsSatisfiedBy(row.Permissions);

// opt-in: bare-action entries ("read") match any feature — a cross-feature wildcard
var satisfied = context.RequiredGrants.IsSatisfiedBy(row.Permissions, allowBareActionShorthand: true);
```

Semantics: AND across required permissions; case-insensitive exact `feature:operation` match;
bare-action shorthand only when explicitly enabled; blank/malformed entries never match; an
empty required set is vacuously satisfied. An `IEnumerable<Permission>` overload exists for
already-parsed entries. Replace hand-rolled `Satisfies`/`MatchesPermission` helpers with this.

## What didn't change

- `ResolveGrantsAsync` and `ShouldBypassAsync` — signatures, semantics, and docs stance
  (pure data lookup; role-gated bypass).
- `OperationGrantResult` / `OperationGrant` shapes and the denied/unrestricted semantics.
- `IOwnedApplicationUser` — still the identity fact for "which company is home" (your provider
  uses it to *query* the right rows) and still the disabled-user backstop (`IsEnabled`). It just
  no longer *implies* access.
- Self-scoped operations (`ISelf*Operation`) — identity-based, never involved home semantics.

## Downstream package impact

`Cirreum.Domain` 4.0.0 removes the orchestrator's home-owner merge (empty granted set →
`Denied`, no exceptions) — see its `MIGRATION-v4.md`. All higher layers are repin-only.
