namespace Cirreum.Contracts.Tests.Authorization;

using Cirreum.Authorization;

public class PermissionSetIsSatisfiedByTests {

	private static PermissionSet Requires(params string[] permissions) =>
		new([.. permissions.Select(Permission.Parse)]);

	// Exact matching ——————————————————————————————————————————

	[Fact]
	public void ExactMatch_SingleRequirement_IsSatisfied() {
		Requires("loans:read")
			.IsSatisfiedBy(["loans:read"])
			.Should().BeTrue();
	}

	[Fact]
	public void AndSemantics_AllRequirementsMet_IsSatisfied() {
		Requires("loans:read", "loans:write")
			.IsSatisfiedBy(["loans:write", "documents:read", "loans:read"])
			.Should().BeTrue();
	}

	[Fact]
	public void AndSemantics_OneRequirementMissing_IsNotSatisfied() {
		Requires("loans:read", "loans:write")
			.IsSatisfiedBy(["loans:read"])
			.Should().BeFalse();
	}

	[Fact]
	public void NoHeldEntries_IsNotSatisfied() {
		Requires("loans:read")
			.IsSatisfiedBy([])
			.Should().BeFalse();
	}

	[Fact]
	public void EmptyRequiredSet_IsVacuouslySatisfied() {
		PermissionSet.Empty
			.IsSatisfiedBy([])
			.Should().BeTrue();
	}

	[Fact]
	public void EmptyRequiredSet_IsSatisfiedRegardlessOfHeldEntries() {
		PermissionSet.Empty
			.IsSatisfiedBy(["anything:atall"])
			.Should().BeTrue();
	}

	// Case-insensitivity ——————————————————————————————————————

	[Theory]
	[InlineData("LOANS:READ")]
	[InlineData("Loans:Read")]
	[InlineData("loans:READ")]
	public void ExactMatch_IsCaseInsensitive(string held) {
		Requires("loans:read")
			.IsSatisfiedBy([held])
			.Should().BeTrue();
	}

	// Bare-action shorthand ———————————————————————————————————

	[Fact]
	public void BareAction_WithoutOptIn_DoesNotMatch() {
		Requires("loans:read")
			.IsSatisfiedBy(["read"])
			.Should().BeFalse();
	}

	[Fact]
	public void BareAction_WithOptIn_MatchesAnyFeature() {
		Requires("loans:read")
			.IsSatisfiedBy(["read"], allowBareActionShorthand: true)
			.Should().BeTrue();
	}

	[Fact]
	public void BareAction_WithOptIn_IsCaseInsensitive() {
		Requires("loans:delete")
			.IsSatisfiedBy(["DELETE"], allowBareActionShorthand: true)
			.Should().BeTrue();
	}

	[Fact]
	public void BareAction_WithOptIn_DifferentOperation_DoesNotMatch() {
		Requires("loans:write")
			.IsSatisfiedBy(["read"], allowBareActionShorthand: true)
			.Should().BeFalse();
	}

	[Fact]
	public void MixedExactAndBareAction_WithOptIn_SatisfiesAndSemantics() {
		Requires("loans:read", "documents:write")
			.IsSatisfiedBy(["loans:read", "write"], allowBareActionShorthand: true)
			.Should().BeTrue();
	}

	// Blank and malformed entries never match —————————————————

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("loans:")]
	[InlineData(":read")]
	[InlineData(":")]
	public void BlankOrMalformedEntry_NeverMatches(string held) {
		Requires("loans:read")
			.IsSatisfiedBy([held], allowBareActionShorthand: true)
			.Should().BeFalse();
	}

	[Fact]
	public void MalformedEntries_AreIgnoredNotFatal() {
		Requires("loans:read")
			.IsSatisfiedBy(["loans:", "loans:read"])
			.Should().BeTrue();
	}

	[Fact]
	public void BareActionWithWhitespace_WithOptIn_IsTrimmedAndMatches() {
		Requires("loans:read")
			.IsSatisfiedBy([" read "], allowBareActionShorthand: true)
			.Should().BeTrue();
	}

	[Fact]
	public void NullHeldEntries_Throws() {
		var act = () => Requires("loans:read").IsSatisfiedBy((IEnumerable<string>)null!);
		act.Should().Throw<ArgumentNullException>();
	}

	// Parsed-Permission overload ——————————————————————————————

	[Fact]
	public void ParsedOverload_AllRequirementsMet_IsSatisfied() {
		Requires("loans:read", "loans:write")
			.IsSatisfiedBy([new Permission("Loans", "Read"), new Permission("loans", "WRITE")])
			.Should().BeTrue();
	}

	[Fact]
	public void ParsedOverload_MissingRequirement_IsNotSatisfied() {
		Requires("loans:read", "loans:write")
			.IsSatisfiedBy([new Permission("loans", "read")])
			.Should().BeFalse();
	}

	[Fact]
	public void ParsedOverload_EmptyRequiredSet_IsVacuouslySatisfied() {
		PermissionSet.Empty
			.IsSatisfiedBy(Array.Empty<Permission>())
			.Should().BeTrue();
	}

	[Fact]
	public void ParsedOverload_NullHeldPermissions_Throws() {
		var act = () => Requires("loans:read").IsSatisfiedBy((IEnumerable<Permission>)null!);
		act.Should().Throw<ArgumentNullException>();
	}
}
