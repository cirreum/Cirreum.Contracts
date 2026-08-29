namespace Cirreum.Contracts.Tests.Authorization;

using Cirreum.Authorization.Operations.Grants;
using System.Text.Json;

/// <summary>
/// Round-trip proofs for <see cref="OperationGrant"/>. A grant is cached by value, so a serializing
/// cache provider deserializes it on every hit while a miss returns the resolver's instance directly.
/// Both paths must present the same shape to handlers.
/// </summary>
public class OperationGrantSerializationTests {

	private static OperationGrant RoundTrip(OperationGrant grant) =>
		JsonSerializer.Deserialize<OperationGrant>(JsonSerializer.Serialize(grant))!;

	[Fact]
	public void Extensions_survive_the_round_trip_as_readable_value_lists() {
		var grant = OperationGrant.ForOwners(
			["company-1"],
			new Dictionary<string, IReadOnlyList<string>> {
				["sessions"] = ["round-5", "round-6"],
				["investorCodes"] = ["SSV1"],
			});

		var restored = RoundTrip(grant);

		restored.Extensions.Should().NotBeNull();
		restored.Extensions!["sessions"].Should().Equal("round-5", "round-6");
		restored.Extensions["investorCodes"].Should().Equal("SSV1");
	}

	[Fact]
	public void Absent_extensions_survive_the_round_trip_as_null() {
		RoundTrip(OperationGrant.ForOwners(["company-1"])).Extensions.Should().BeNull();
	}

	[Fact]
	public void Bounded_owner_set_survives_the_round_trip() {
		var restored = RoundTrip(OperationGrant.ForOwners(["company-1", "company-2"]));

		restored.IsBounded.Should().BeTrue();
		restored.Contains("company-2").Should().BeTrue();
		restored.Contains("company-3").Should().BeFalse();
	}

	[Fact]
	public void Denied_and_Unrestricted_survive_the_round_trip() {
		RoundTrip(OperationGrant.Denied).IsDenied.Should().BeTrue();
		RoundTrip(OperationGrant.Unrestricted).IsUnrestricted.Should().BeTrue();
	}
}
