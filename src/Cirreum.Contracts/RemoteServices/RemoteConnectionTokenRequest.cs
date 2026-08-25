namespace Cirreum.RemoteServices;

/// <summary>
/// Describes the connection an access token is being obtained for.
/// </summary>
/// <param name="EndpointUri">The absolute Uri of the remote endpoint.</param>
/// <param name="Scopes">
/// The scopes the connection's options declare. Empty when the application named none.
/// </param>
/// <param name="ConnectionName">The name of the connection type the token is for.</param>
public sealed record RemoteConnectionTokenRequest(
	Uri EndpointUri,
	IReadOnlyList<string> Scopes,
	string ConnectionName);
