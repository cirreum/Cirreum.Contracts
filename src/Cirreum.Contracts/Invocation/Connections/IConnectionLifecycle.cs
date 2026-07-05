namespace Cirreum.Invocation.Connections;

/// <summary>
/// App-side hook invoked by the source adapter at connection lifecycle boundaries
/// (long-lived sources only). Apps register an implementation via DI; the adapter
/// resolves and invokes it (if any) at upgrade and disconnect.
/// </summary>
/// <remarks>
/// <para>
/// Long-lived invocation sources only (SignalR, raw WebSocket, gRPC streaming). Stateless
/// sources (HTTP, gRPC unary, queue handlers) do not invoke this hook — they have no
/// connection lifecycle.
/// </para>
/// <para>
/// <strong>Live-state vs. cleanup.</strong> <see cref="OnConnectedAsync"/> and
/// <see cref="OnDisconnectedAsync"/> bracket the connection's <em>live</em> state — the
/// window from a successful upgrade to a disconnect. A connection whose upgrade is
/// <em>rejected</em> (a lifecycle returns <see langword="false"/> or throws) or that
/// <em>faults</em> during establishment never became live, so it does not receive
/// <see cref="OnDisconnectedAsync"/> — that hook is for observing an actual disconnect
/// (its <see cref="DisconnectInfo"/> would be meaningless for a connection that never
/// connected). For cleanup that must run on <em>every</em> exit — including a rejected or
/// faulted establishment — do not rely on <see cref="OnDisconnectedAsync"/>; tie it to
/// <see cref="IInvocationConnection.Aborted"/>, which cancels whenever the connection is
/// torn down regardless of whether it went live. (The framework's own connection registry
/// does exactly this.)
/// </para>
/// </remarks>
public interface IConnectionLifecycle {

	/// <summary>
	/// Called after upgrade completes and identity context has been copied to the
	/// connection. Return <see langword="false"/> or throw to reject the connection
	/// (the adapter aborts the upgrade; client sees normal upgrade-rejection).
	/// </summary>
	/// <remarks>
	/// Runs inside a synthetic invocation scope established by the adapter so that
	/// <c>IUserStateAccessor</c> and other ambient consumers work normally inside the
	/// hook.
	/// </remarks>
	ValueTask<bool> OnConnectedAsync(IInvocationConnection connection, CancellationToken cancellationToken);

	/// <summary>
	/// Called after the adapter detects a disconnect of a <em>live</em> connection, before
	/// connection resources are disposed. Receives a <see cref="DisconnectInfo"/> describing
	/// whether the close was graceful, any reported exception, and a human-readable reason.
	/// Exceptions thrown from this hook are absorbed by the framework.
	/// </summary>
	/// <remarks>
	/// Fires only for a connection that completed its upgrade and went live. It does
	/// <strong>not</strong> fire when establishment is rejected or faults (see the
	/// interface-level remarks) — bind cleanup that must survive those paths to
	/// <see cref="IInvocationConnection.Aborted"/> instead.
	/// </remarks>
	/// <param name="connection">The connection that disconnected.</param>
	/// <param name="info">
	/// Adapter-populated disconnect circumstances. See <see cref="DisconnectInfo"/> for
	/// per-transport mappings (SignalR exception, WebSocket close status, gRPC status).
	/// </param>
	/// <param name="cancellationToken">
	/// Connection-tied cancellation; already canceled by the time this hook runs (the
	/// disconnect is what fired it). Provided for API symmetry with
	/// <see cref="OnConnectedAsync"/> and to flow into any cancellable cleanup the hook
	/// chooses to perform.
	/// </param>
	ValueTask OnDisconnectedAsync(
		IInvocationConnection connection,
		DisconnectInfo info,
		CancellationToken cancellationToken);

}
