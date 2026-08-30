namespace Cirreum.RemoteServices.Connections;

/// <summary>
/// Describes the connection a credential is being obtained for.
/// </summary>
public sealed record RemoteConnectionCredentialRequest {

	/// <summary>
	/// Gets the absolute Uri of the remote endpoint.
	/// </summary>
	public required Uri EndpointUri { get; init; }

	/// <summary>
	/// Gets the scopes the connection's options declare. Empty when the application named none.
	/// </summary>
	public required IReadOnlyList<string> Scopes { get; init; }

	/// <summary>
	/// Gets the connection type the credential is for, which is also the key a source may be
	/// registered against.
	/// </summary>
	public required Type ConnectionType { get; init; }

}
