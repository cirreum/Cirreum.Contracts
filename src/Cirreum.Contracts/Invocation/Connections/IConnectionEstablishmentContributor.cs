namespace Cirreum.Invocation.Connections;

/// <summary>
/// Cross-track seam for propagating ambient state onto a long-lived connection at
/// establishment. Transport adapters (SignalR's hub filter, the WebSocket orchestrator)
/// resolve all registered contributors at connection upgrade and invoke each, letting any
/// track that owns per-connection state copy its slots from the upgrade request onto the
/// connection without the transport adapter knowing what those slots mean.
/// </summary>
/// <remarks>
/// <para>
/// The contract is deliberately <c>HttpContext</c>-free so it can live in
/// <c>Cirreum.Contracts</c> alongside the rest of the Invocation abstractions — the
/// upgrade-time bag is passed as a generic <see cref="IDictionary{TKey, TValue}"/>, which
/// is the shape of <c>HttpContext.Items</c> (the current WS/SignalR upgrade path) and of
/// any future non-HTTP upgrade flow.
/// </para>
/// <para>
/// The interface knows nothing about authentication, authorization, or any specific slot.
/// Authentication registers an internal contributor that copies its own slots
/// (<c>AuthenticationContextKeys.AuthenticatedScheme</c>,
/// <c>AuthenticationContextKeys.ApplicationUserCache</c>) from the upgrade items to
/// <see cref="IInvocationConnection.Items"/>; future tracks (tenancy, feature flags,
/// telemetry baggage) register their own without changing Invocation.
/// </para>
/// </remarks>
public interface IConnectionEstablishmentContributor {

	/// <summary>
	/// Invoked once per connection at establishment, after authentication has succeeded
	/// and before the first invocation is dispatched. Implementations copy whichever slots
	/// they own from <paramref name="upgradeItems"/> onto
	/// <paramref name="connection"/>'s <see cref="IInvocationConnection.Items"/>.
	/// </summary>
	/// <param name="upgradeItems">The upgrade request's ambient item bag (e.g.
	/// <c>HttpContext.Items</c> at the WS/SignalR upgrade).</param>
	/// <param name="connection">The newly established connection whose
	/// <see cref="IInvocationConnection.Items"/> receives the propagated slots.</param>
	void OnEstablishing(
		IDictionary<object, object?> upgradeItems,
		IInvocationConnection connection);

}
