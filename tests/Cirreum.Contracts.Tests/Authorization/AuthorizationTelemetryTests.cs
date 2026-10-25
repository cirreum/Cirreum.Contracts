namespace Cirreum.Contracts.Tests.Authorization;

using Cirreum.Authorization;
using Cirreum.Diagnostics;
using System.Diagnostics.Metrics;

public class AuthorizationTelemetryTests {

	private sealed record Measurement(Instrument Instrument, double Value, KeyValuePair<string, object?>[] Tags);

	/// <summary>Captures histogram measurements from the authorization meter carrying <paramref name="resourceType"/>.</summary>
	private static (MeterListener Listener, List<Measurement> Measurements) Listen(string resourceType) {
		var measurements = new List<Measurement>();
		var listener = new MeterListener {
			InstrumentPublished = (instrument, l) => {
				if (instrument.Meter.Name == CirreumTelemetry.Meters.Authorization) {
					l.EnableMeasurementEvents(instrument);
				}
			}
		};
		listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) => {
			var copy = tags.ToArray();
			if (copy.Any(t => t.Key == AuthorizationTelemetry.ResourceTypeTag && Equals(t.Value, resourceType))) {
				lock (measurements) {
					measurements.Add(new(instrument, value, copy));
				}
			}
		});
		listener.Start();
		return (listener, measurements);
	}

	[Fact]
	public void RecordDuration_RecordsSecondsOnTheEvaluationHistogram() {
		var (listener, measurements) = Listen("duration-probe");
		using (listener) {
			AuthorizationTelemetry.RecordDuration(
				activity: null, "duration-probe", durationMs: 250, AuthorizationTelemetry.DecisionPass);
		}

		var measurement = measurements.Should().ContainSingle().Subject;
		measurement.Instrument.Name.Should().Be("cirreum.authz.evaluation.duration");
		measurement.Instrument.Unit.Should().Be("s");
		measurement.Value.Should().Be(0.25);
	}

	[Fact]
	public void RecordGrantResolution_RecordsSecondsOnTheGrantResolutionHistogram() {
		var (listener, measurements) = Listen("grant-probe");
		using (listener) {
			AuthorizationTelemetry.RecordGrantResolution(
				feature: "orders", "grant-probe", AuthorizationTelemetry.GrantLevelL2, durationMs: 40);
		}

		var measurement = measurements.Should().ContainSingle().Subject;
		measurement.Instrument.Name.Should().Be("cirreum.authz.grant.resolution.duration");
		measurement.Instrument.Unit.Should().Be("s");
		measurement.Value.Should().Be(0.04);
	}

	[Fact]
	public void DurationHistograms_CarryTheSharedSecondsBuckets() {
		var histograms = new List<Instrument>();
		using var listener = new MeterListener {
			InstrumentPublished = (instrument, _) => {
				if (instrument.Meter.Name == CirreumTelemetry.Meters.Authorization && instrument.Name.EndsWith(".duration")) {
					lock (histograms) {
						histograms.Add(instrument);
					}
				}
			}
		};
		listener.Start();
		AuthorizationTelemetry.RecordGrantResolution(null, null, AuthorizationTelemetry.GrantLevelBypass);

		histograms.Should().HaveCount(2).And.AllSatisfy(h =>
			((Histogram<double>)h).Advice!.HistogramBucketBoundaries
				.Should().Equal(CirreumTelemetry.Histograms.DurationAdvice.HistogramBucketBoundaries));
	}

}
