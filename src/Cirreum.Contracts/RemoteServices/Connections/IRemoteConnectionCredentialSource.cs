namespace Cirreum.RemoteServices.Connections;

/// <summary>
/// Supplies the credential attached to outbound remote connections whose options do not specify
/// one explicitly.
/// </summary>
/// <remarks>
/// Registered by the host runtime against whatever identity source that host provides. A source
/// may also be registered keyed by connection type, in which case it is preferred over the
/// unkeyed registration for that connection.
/// </remarks>
public interface IRemoteConnectionCredentialSource {

	/// <summary>
	/// Resolve the credential for the connection described by <paramref name="request"/>.
	/// </summary>
	/// <param name="request">The connection the credential is being obtained for.</param>
	/// <param name="cancellationToken">Cancellation token for the resolution.</param>
	/// <returns>
	/// The credential, <see cref="AuthorizationHeaderSettings.None"/> to connect without one, or
	/// <see langword="null"/> when none is available.
	/// </returns>
	ValueTask<AuthorizationHeaderSettings?> GetCredentialAsync(
		RemoteConnectionCredentialRequest request,
		CancellationToken cancellationToken = default);

}
