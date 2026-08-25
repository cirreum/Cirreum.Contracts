namespace Cirreum.Contracts.Tests.RemoteServices;

using Cirreum.RemoteServices;

public class RemoteConnectionOptionsTests {

	private static RemoteConnectionOptions Options(string endpoint = "https://example.test/hub") =>
		new("TestApp", new Uri(endpoint));

	private static AuthorizationHeaderSettings Bearer(string value) =>
		new() { Scheme = "Bearer", Value = value };

	// Equality — the registration dedup rule depends on it ——————————————————

	[Fact]
	public void SameValues_AreEqual() {
		Options().Should().Be(Options());
	}

	[Fact]
	public void SameValues_ShareHashCode() {
		Options().GetHashCode().Should().Be(Options().GetHashCode());
	}

	[Fact]
	public void DifferentEndpoint_AreNotEqual() {
		Options().Should().NotBe(Options("https://example.test/other"));
	}

	[Fact]
	public void DifferentApplicationName_AreNotEqual() {
		Options().Should().NotBe(new RemoteConnectionOptions("OtherApp", new Uri("https://example.test/hub")));
	}

	[Fact]
	public void DifferentReconnectSetting_AreNotEqual() {
		var left = Options();
		var right = Options();
		right.Reconnect = false;

		left.Should().NotBe(right);
	}

	[Fact]
	public void DifferentReconnectMaxDelay_AreNotEqual() {
		var left = Options();
		var right = Options();
		right.ReconnectMaxDelay = TimeSpan.FromSeconds(5);

		left.Should().NotBe(right);
	}

	[Fact]
	public void DifferentAuthorizationHeader_AreNotEqual() {
		var left = Options();
		var right = Options();
		right.AuthorizationHeader = new AuthorizationHeaderSettings { Scheme = "Bearer", Value = "token" };

		left.Should().NotBe(right);
	}

	[Fact]
	public void ExplicitlyPublicAndDefaultPosture_AreNotEqual() {
		var left = Options();
		var right = Options();
		right.AuthorizationHeader = AuthorizationHeaderSettings.None;

		left.Should().NotBe(right);
	}

	[Fact]
	public void SameCredentialProviderInstance_AreEqual() {
		Func<CancellationToken, ValueTask<AuthorizationHeaderSettings?>> provider =
			_ => ValueTask.FromResult<AuthorizationHeaderSettings?>(Bearer("token"));
		var left = Options();
		var right = Options();
		left.CredentialProvider = provider;
		right.CredentialProvider = provider;

		left.Should().Be(right);
	}

	[Fact]
	public void DifferentCredentialProviderInstances_AreNotEqual() {
		var left = Options();
		var right = Options();
		left.CredentialProvider = _ => ValueTask.FromResult<AuthorizationHeaderSettings?>(Bearer("token"));
		right.CredentialProvider = _ => ValueTask.FromResult<AuthorizationHeaderSettings?>(Bearer("token"));

		left.Should().NotBe(right);
	}

	[Fact]
	public void DifferentScopes_AreNotEqual() {

		var a = Options();
		a.Scopes = ["api://one/.default"];

		var b = Options();
		b.Scopes = ["api://two/.default"];

		a.Should().NotBe(b);

	}

	[Fact]
	public void SameScopes_AreEqual() {

		var a = Options();
		a.Scopes = ["api://one/.default", "api://one/extra"];

		var b = Options();
		b.Scopes = ["api://one/.default", "api://one/extra"];

		// Distinct list instances carrying the same scopes are the same registration.
		a.Should().Be(b);
		a.GetHashCode().Should().Be(b.GetHashCode());

	}

	[Fact]
	public void ScopeOrder_IsSignificant() {

		var a = Options();
		a.Scopes = ["one", "two"];

		var b = Options();
		b.Scopes = ["two", "one"];

		// Order is carried through to the token request verbatim, so it is part of the value.
		a.Should().NotBe(b);

	}

	[Fact]
	public void DeclaringScopes_DiffersFromDeclaringNone() {

		var declared = Options();
		declared.Scopes = ["api://one/.default"];

		// The empty default means "the host was told no audience" — a different registration,
		// not an unset field that happens to match.
		declared.Should().NotBe(Options());

	}

	[Fact]
	public void NullComparand_IsNotEqual() {
		Options().Equals(null).Should().BeFalse();
	}

	[Fact]
	public void Operators_MatchEquals() {
		var left = Options();
		var right = Options();

		(left == right).Should().BeTrue();
		(left != right).Should().BeFalse();
		(left == null).Should().BeFalse();
		((RemoteConnectionOptions?)null == null).Should().BeTrue();
	}

	// Defaults ——————————————————————————————————————————————————

	[Fact]
	public void Defaults_ReconnectEnabledWithThirtySecondCeiling() {
		var options = new RemoteConnectionOptions();

		options.Reconnect.Should().BeTrue();
		options.ReconnectMaxDelay.Should().Be(TimeSpan.FromSeconds(30));
	}

	[Fact]
	public void Defaults_NoCredentialPostureIsSelected() {
		var options = new RemoteConnectionOptions();

		options.AuthorizationHeader.Should().BeNull();
		options.CredentialProvider.Should().BeNull();
	}

	[Fact]
	public void ParameterlessConstructor_DerivesApplicationName() {
		new RemoteConnectionOptions().ApplicationName.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void DefaultEndpointUri_IsNotAbsolute() {
		new RemoteConnectionOptions().EndpointUri.IsAbsoluteUri.Should().BeFalse();
	}

}
