namespace Cirreum.Invocation.Connections;

/// <summary>
/// Per-server index of active long-lived <see cref="IInvocationConnection"/> instances,
/// keyed by connection id and queryable by authenticated subject. Implemented and fed by
/// <c>Cirreum.Services.Server</c> (a framework-shipped <c>IConnectionLifecycle</c> feeds
/// it through the source adapters' connect/disconnect hooks) and consumed by the
/// framework-shipped connection-terminator handler in the same package to act on
/// <c>CredentialRevoked</c>, <c>UserAccountDisabled</c>, and
/// <c>SessionTerminationRequested</c> auth events (from Cirreum.Authentication.Events).
/// </summary>
/// <remarks>
/// <para>
/// The registry plus the connection-terminator together replace the
/// app-level wiring recipe from the deprecated actor model — the framework now ships
/// cross-source connection termination as built-in functionality.
/// </para>
/// <para>
/// Scope is per-process; cross-replica termination flows via the auth event bus
/// (published once, fanned out to every replica's
/// in-memory handler). Each replica's terminator queries its local registry for matches.
/// </para>
/// </remarks>
public interface IInvocationConnectionRegistry {

	/// <summary>
	/// Registers a newly-upgraded connection. Idempotent on
	/// <see cref="IInvocationConnection.ConnectionId"/>.
	/// </summary>
	void Register(IInvocationConnection connection);

	/// <summary>
	/// Removes a connection from the registry (typically called from the source
	/// adapter's disconnect path). Idempotent — unknown connection ids are no-ops.
	/// </summary>
	void Unregister(string connectionId);

	/// <summary>
	/// Returns all currently-registered connections whose authenticated subject
	/// matches <paramref name="subject"/>. Empty when no matches. Snapshot semantics —
	/// safe to enumerate concurrently with registrations/unregistrations.
	/// </summary>
	/// <remarks>
	/// Implementations resolve each connection's subject from its <c>EffectiveUser</c>
	/// (see <see cref="InvocationConnectionExtensions"/>), so a connection promoted
	/// mid-flight via Two-Phase Auth matches under its promoted identity.
	/// </remarks>
	IEnumerable<IInvocationConnection> FindBySubject(string subject);

	/// <summary>
	/// Returns all currently-registered connections regardless of subject. Snapshot
	/// semantics. Used by diagnostics and operator tooling; not on the hot path.
	/// </summary>
	IEnumerable<IInvocationConnection> All();

	/// <summary>
	/// Looks up a specific connection by id. Returns <see langword="null"/> when not
	/// registered.
	/// </summary>
	IInvocationConnection? Find(string connectionId);

}
