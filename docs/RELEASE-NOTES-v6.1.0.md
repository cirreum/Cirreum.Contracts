# Cirreum.Contracts 6.1.0 — authorization durations in seconds

## Why this release exists

Every Cirreum duration histogram recorded milliseconds. The OpenTelemetry semantic conventions
specify seconds for durations, and the .NET built-in meters follow them —
`http.server.request.duration`, `http.client.request.duration`, and `kestrel.connection.duration`
are all `s`. A dashboard that plotted a Cirreum histogram beside an ASP.NET Core one was mixing
units.

Contracts carries the lowest-layer Cirreum histograms, so the move to seconds starts here.

## What changed

| Before (`ms`) | After (`s`) |
| ------------- | ----------- |
| `cirreum.authz.duration` | `cirreum.authz.evaluation.duration` |
| `cirreum.authz.grant.duration` | `cirreum.authz.grant.resolution.duration` |

Both histograms take their unit and bucket boundaries from `CirreumTelemetry.Histograms` in
`Cirreum.Kernel` 2.3.0. The boundaries — `0.005` to `10` seconds — are the ones the ASP.NET Core
and `HttpClient` request-duration histograms use.

## Why the names changed with the unit

An exporter that keys series by name alone, such as Azure Monitor, would keep writing to
`cirreum.authz.duration` with values 1000× smaller and no error. A new name makes old queries
return nothing instead, which is visible.

## Why the bucket boundaries matter

The OpenTelemetry SDK's default histogram boundaries run from `0` to `10000` and are sized for
milliseconds. A seconds histogram recorded against them puts almost every authorization decision
in the first bucket, and the percentiles report nothing useful. `CirreumTelemetry.Histograms`
supplies seconds-sized boundaries as instrument advice, so no exporter configuration is needed.

## Compatibility

* **No signature changes.** `AuthorizationTelemetry.DurationMetric` and `GrantDurationMetric` keep
  their names; their values changed. `const` values are compiled into the assemblies that read
  them, so an application referencing either constant sees the new name after it is rebuilt.
* **`RecordDuration` and `RecordGrantResolution` still take milliseconds** and convert when they
  record. Callers need no change.
* **Dashboards and alerts** on the old names need the new names and a unit of seconds — a 250 ms
  threshold is now `0.25`.
