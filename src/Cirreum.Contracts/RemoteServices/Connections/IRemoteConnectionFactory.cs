namespace Cirreum.RemoteServices.Connections;

/// <summary>
/// Creates per-session instances of a registered remote connection type.
/// </summary>
/// <typeparam name="TConnection">The registered connection type.</typeparam>
/// <remarks>
/// <para>
/// Each created instance is owned by the caller, which connects, uses, and disposes it. This
/// is the counterpart to registering a single connection for the lifetime of the application.
/// </para>
/// </remarks>
public interface IRemoteConnectionFactory<TConnection> where TConnection : IRemoteConnection {

	/// <summary>
	/// Create a new, unconnected instance using the registered options.
	/// </summary>
	/// <param name="configure">
	/// An optional delegate that adjusts a copy of the registered options for this instance.
	/// </param>
	TConnection Create(Action<RemoteConnectionOptions>? configure = null);

}
