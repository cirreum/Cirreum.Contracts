namespace Cirreum.RemoteServices;

/// <summary>
/// Supplies the access token attached to outbound remote connections whose options do not
/// specify credentials explicitly.
/// </summary>
/// <remarks>
/// Registered by the host runtime against whatever identity source that host provides.
/// </remarks>
public interface IRemoteConnectionTokenSource {

	/// <summary>
	/// Resolve the access token for the connection described by <paramref name="request"/>, or
	/// <see langword="null"/> when no token is available.
	/// </summary>
	/// <param name="request">The connection the token is being obtained for.</param>
	/// <param name="cancellationToken">Cancellation token for the resolution.</param>
	ValueTask<string?> GetAccessTokenAsync(
		RemoteConnectionTokenRequest request,
		CancellationToken cancellationToken = default);

}
