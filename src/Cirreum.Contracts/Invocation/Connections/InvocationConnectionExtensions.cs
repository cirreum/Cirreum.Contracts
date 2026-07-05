namespace Cirreum.Invocation.Connections;

using Cirreum.Authentication;
using System.Security.Claims;

/// <summary>
/// The connection-ownership surface of <see cref="IInvocationConnection"/> — extension
/// members answering "who owns this connection <em>now</em>", uniformly for every
/// implementation. Deliberately extensions rather than interface members: ownership
/// resolution must be identical across transports (nothing to override), the members are
/// visible on concrete connection types without an interface cast, and test doubles get
/// the real logic for free.
/// </summary>
public static class InvocationConnectionExtensions {

	extension(IInvocationConnection connection) {

		/// <summary>
		/// The Two-Phase Auth promoted principal, or <see langword="null"/> when the
		/// connection has not been promoted. The nullable primitive behind
		/// <c>EffectiveUser</c> / <c>IsUserPromoted</c> — read this when you need the
		/// promoted identity <em>as distinct from</em> the upgrade-time
		/// <see cref="IInvocationConnection.User"/> (audit trails and diagnostics that
		/// report both identities of a promoted connection).
		/// </summary>
		public ClaimsPrincipal? PromotedUser =>
			connection.Items.TryGetValue(AuthenticationContextKeys.PromotedPrincipal, out var promoted)
				&& promoted is ClaimsPrincipal principal
					? principal
					: null;

		/// <summary>
		/// The connection's <em>effective</em> principal: the Two-Phase Auth promoted
		/// principal when one has been stamped (via <c>connection.Promote(...)</c> in
		/// <c>Cirreum.Runtime.AuthenticationProvider</c>), otherwise the upgrade-time
		/// <see cref="IInvocationConnection.User"/>. Consumers that reason about "who owns
		/// this connection" — the per-invocation contexts, the connection registry's
		/// subject lookup, the connection-terminator's session matching, app lifecycle
		/// hooks and diagnostics — should read this member, not
		/// <see cref="IInvocationConnection.User"/>.
		/// </summary>
		/// <remarks>
		/// <see cref="IInvocationConnection.User"/> stays immutable post-upgrade by
		/// contract; promotion decorates the connection through its
		/// <see cref="IInvocationConnection.Items"/> bag (under
		/// <see cref="AuthenticationContextKeys.PromotedPrincipal"/>) rather than mutating
		/// it. Re-promotion overwrites the slot, so this member always reflects the most
		/// recent promotion. Framework adapters snapshot this value per invocation, so a
		/// promotion takes effect from the next invocation on the connection.
		/// </remarks>
		public ClaimsPrincipal EffectiveUser => connection.PromotedUser ?? connection.User;

		/// <summary>
		/// <see langword="true"/> when the connection has been promoted mid-flight via
		/// Two-Phase Auth (an anonymous-pending-auth connection whose human was identified
		/// in-band); <see langword="false"/> when <c>EffectiveUser</c> is still the
		/// upgrade-time <see cref="IInvocationConnection.User"/>.
		/// </summary>
		public bool IsUserPromoted => connection.PromotedUser is not null;

	}

}
