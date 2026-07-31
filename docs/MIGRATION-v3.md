# Cirreum.Contracts v2 → v3 Migration

v3 carries one breaking change: the Stage 3 authorization telemetry constant is renamed — in
both its C# name and its emitted wire value — as part of the framework-wide "policy authorizer"
vocabulary correction (paired with `Cirreum.Domain` 3.0.0, which renames `IPolicyValidator` to
`IPolicyAuthorizer`).

"Validator" in Cirreum means FluentValidation property validation — the Conductor `Validation`
intercept's stage. The Stage 3 extension point performs **authorization**: it runs inside the
authorization evaluator, denies with `ForbiddenAccessException`, and participates in deny
telemetry. The vocabulary now says so everywhere.

---

## 1. `AuthorizationTelemetry.StepPolicyValidator` → `StepPolicyAuthorizer`

| | Before | After |
|---|---|---|
| C# constant | `AuthorizationTelemetry.StepPolicyValidator` | `AuthorizationTelemetry.StepPolicyAuthorizer` |
| Wire value (`cirreum.authz.step` tag / metric dimension) | `policy-validator` | `policy-authorizer` |

### Migration

Code: find/replace the constant name — a compile error until fixed.

Telemetry: any dashboard, alert, or saved query filtering on the step dimension value
`policy-validator` must be updated to `policy-authorizer`. Historical data keeps the old value;
a transition-window query should match both:

```kusto
| where step in ("policy-validator", "policy-authorizer")
```

`StagePolicy` (`"policy"`) is unchanged — only the step constant moves.

---

## What didn't change

- Every other `AuthorizationTelemetry` constant, stage, and wire value.
- All authorization contracts (`IAuthorizer<T>`, `IAuthorizationEvaluator`, grants, resources).
- `IResourceAccessEvaluator` gains documentation (the authorization-pipeline dependency is now
  stated on the contract) but its surface is unchanged.

## Downstream package impact

`Cirreum.Domain` 3.0.0 consumes the renamed constant and carries the interface renames this
vocabulary correction exists for — see its `MIGRATION-v3.md` for the app-facing changes
(`IPolicyValidator` → `IPolicyAuthorizer`, `ValidateAsync` → `EvaluateAsync`,
`AttributeValidatorBase<TAttribute>` → `AttributePolicyAuthorizerBase<TAttribute>`).
