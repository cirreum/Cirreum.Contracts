namespace Cirreum.Contracts.Tests.RemoteServices;

using Cirreum.RemoteServices;

public class RemoteConnectionOptionsTests {

	private static RemoteConnectionOptions Options(string endpoint = "https://example.test/hub") =>
		new("TestApp", new Uri(endpoint));

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
	public void SameAccessTokenProviderInstance_AreEqual() {
		Func<CancellationToken, ValueTask<string?>> provider = _ => ValueTask.FromResult<string?>("token");
		var left = Options();
		var right = Options();
		left.AccessTokenProvider = provider;
		right.AccessTokenProvider = provider;

		left.Should().Be(right);
	}

	[Fact]
	public void DifferentAccessTokenProviderInstances_AreNotEqual() {
		var left = Options();
		var right = Options();
		left.AccessTokenProvider = _ => ValueTask.FromResult<string?>("token");
		right.AccessTokenProvider = _ => ValueTask.FromResult<string?>("token");

		left.Should().NotBe(right);
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
		options.AccessTokenProvider.Should().BeNull();
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
