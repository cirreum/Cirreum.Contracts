# Cirreum.Contracts 4.5.0 — presence refresh limits become part of the contract

## Why this release exists

`UserPresenceMonitorOptions` already defined the runtime behavior around presence refresh
frequency: one minute by default, `0` to disable monitoring, and enabled intervals of five
seconds or less rejected in favor of the default.

What it did not expose was the boundary itself.

Consumers configuring presence through higher-level packages therefore had to repeat the
literal `5_000` whenever they wanted to validate an explicitly supplied refresh interval:

```csharp
if (refreshInterval is > 0 and <= 5_000) {
    // invalid
}
```

That makes a framework rule an implementation detail duplicated outside the contract that
owns it. 4.5.0 promotes that boundary to public API so configuration surfaces can validate
against the same definition as the presence monitor.

## What's new

`UserPresenceMonitorOptions` now exposes:

```csharp
public const int MinimumRefreshInterval = 5_000;
```

Together with the existing default:

```csharp
public const int DefaultRefreshInterval = 60_000;
```

the presence timing contract is now fully named:

| Value                        | Semantics                                  |
| ---------------------------- | ------------------------------------------ |
| `0`                          | presence monitoring disabled               |
| `1`–`MinimumRefreshInterval` | invalid enabled interval                   |
| `> MinimumRefreshInterval`   | valid enabled interval                     |
| `DefaultRefreshInterval`     | default when no valid override is supplied |

Higher-level configuration APIs can now validate without embedding their own copy of the
framework limit:

```csharp
if (refreshInterval is < 0 ||
    refreshInterval is > 0
        and <= UserPresenceMonitorOptions.MinimumRefreshInterval) {

    throw new ArgumentOutOfRangeException(nameof(refreshInterval));
}
```

The XML documentation on `UserPresenceMonitorOptions` has also been tightened to make the
distinction explicit: `0` is the intentional disabled state, while positive values through
five seconds are invalid and fall back to the one-minute default.

## Compatibility

* **Purely additive** — one new public constant on `UserPresenceMonitorOptions`.
* `DefaultRefreshInterval` remains `60_000` milliseconds.
* `RefreshInterval` retains its existing default and runtime semantics.
* No existing consumer or implementer changes are required.

## See also

* `UserPresenceMonitorOptions.MinimumRefreshInterval` — the shared lower bound for enabled
  presence refresh intervals.
* `UserPresenceMonitorOptions.DefaultRefreshInterval` — the existing one-minute default.
