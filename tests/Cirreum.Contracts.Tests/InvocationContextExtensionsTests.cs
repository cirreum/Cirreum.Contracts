namespace Cirreum.Contracts.Tests;

using Cirreum.Authentication;
using Cirreum.Invocation;
using Cirreum.Invocation.Connections;

public class InvocationContextExtensionsTests {

	private static IInvocationContext CreateInvocation(
		IDictionary<object, object?>? items = null,
		IInvocationConnection? connection = null) {

		var invocation = Substitute.For<IInvocationContext>();
		invocation.Items.Returns(items ?? new Dictionary<object, object?>());
		invocation.Connection.Returns(connection);
		return invocation;
	}

	private static IInvocationConnection CreateConnection(IDictionary<object, object?>? items = null) {
		var connection = Substitute.For<IInvocationConnection>();
		connection.Items.Returns(items ?? new Dictionary<object, object?>());
		return connection;
	}

	// AuthenticatedScheme
	// -------------------------------------------------------------

	[Fact]
	public void AuthenticatedScheme_ReturnsStampedValue() {
		var invocation = CreateInvocation(new Dictionary<object, object?> {
			[AuthenticationContextKeys.AuthenticatedScheme] = "descope"
		});

		invocation.AuthenticatedScheme.Should().Be("descope");
	}

	[Fact]
	public void AuthenticatedScheme_ReturnsNull_WhenUnstamped() {
		var invocation = CreateInvocation();

		invocation.AuthenticatedScheme.Should().BeNull();
	}

	[Fact]
	public void AuthenticatedScheme_ReturnsNull_WhenSlotIsNotAString() {
		var invocation = CreateInvocation(new Dictionary<object, object?> {
			[AuthenticationContextKeys.AuthenticatedScheme] = 42
		});

		invocation.AuthenticatedScheme.Should().BeNull();
	}

	[Fact]
	public void AuthenticatedScheme_DoesNotReadTheConnection() {
		var connection = CreateConnection(new Dictionary<object, object?> {
			[AuthenticationContextKeys.AuthenticatedScheme] = "ApiKey:Header"
		});
		var invocation = CreateInvocation(connection: connection);

		invocation.AuthenticatedScheme.Should().BeNull();
	}

	// OriginScheme
	// -------------------------------------------------------------

	[Fact]
	public void OriginScheme_ReturnsNull_WhenUnstamped() {
		var invocation = CreateInvocation();

		invocation.OriginScheme.Should().BeNull();
	}

	[Fact]
	public void OriginScheme_ReadsInvocationItems() {
		var invocation = CreateInvocation(new Dictionary<object, object?> {
			[AuthenticationContextKeys.OriginScheme] = "descope"
		});

		invocation.OriginScheme.Should().Be("descope");
	}

	[Fact]
	public void OriginScheme_DoesNotReadTheConnection() {
		// The per-invocation seed is what carries connection slots into an invocation's
		// snapshot; the property never reaches past the snapshot to the live connection.
		var connection = CreateConnection(new Dictionary<object, object?> {
			[AuthenticationContextKeys.OriginScheme] = "entraWorkforce"
		});
		var invocation = CreateInvocation(
			new Dictionary<object, object?> {
				[AuthenticationContextKeys.OriginScheme] = "descope"
			},
			connection);

		invocation.OriginScheme.Should().Be("descope");
	}

	// EffectiveScheme
	// -------------------------------------------------------------

	[Fact]
	public void EffectiveScheme_ReturnsOrigin_WhenStamped() {
		var invocation = CreateInvocation(new Dictionary<object, object?> {
			[AuthenticationContextKeys.AuthenticatedScheme] = "SessionTicket:Bearer",
			[AuthenticationContextKeys.OriginScheme] = "descope"
		});

		invocation.EffectiveScheme.Should().Be("descope");
	}

	[Fact]
	public void EffectiveScheme_ReturnsAuthenticatedScheme_WhenNoOrigin() {
		var invocation = CreateInvocation(new Dictionary<object, object?> {
			[AuthenticationContextKeys.AuthenticatedScheme] = "ApiKey:Header"
		});

		invocation.EffectiveScheme.Should().Be("ApiKey:Header");
	}

	[Fact]
	public void EffectiveScheme_ReturnsNull_WhenNothingIsStamped() {
		var invocation = CreateInvocation();

		invocation.EffectiveScheme.Should().BeNull();
	}

	[Fact]
	public void EffectiveScheme_DoesNotSeeAPromotionStampedMidInvocation() {
		// Promotion stamps the connection during an invocation whose per-invocation items
		// were seeded beforehand. The promoted identity becomes visible at the NEXT
		// invocation, and the scheme facts travel with the same snapshot — this invocation
		// keeps reporting the subject it was constructed with.
		var connectionItems = new Dictionary<object, object?>();
		var invocation = CreateInvocation(
			new Dictionary<object, object?> {
				[AuthenticationContextKeys.AuthenticatedScheme] = "ApiKey:Header"
			},
			CreateConnection(connectionItems));

		invocation.EffectiveScheme.Should().Be("ApiKey:Header");

		connectionItems[AuthenticationContextKeys.OriginScheme] = "entraWorkforce";

		invocation.EffectiveScheme.Should().Be("ApiKey:Header");
	}

}
