namespace Cirreum.RemoteServices;

/// <summary>
/// Configured options for a given remote connection.
/// </summary>
/// <remarks>
/// <para>
/// Uses the entry assembly's name as the <see cref="ApplicationName"/> if not provided by
/// the caller.
/// </para>
/// <para>
/// Credentials resolve in a fixed order: <see cref="CredentialProvider"/> when set, then
/// <see cref="AuthorizationHeader"/>, then the ambient <see cref="IRemoteConnectionCredentialSource"/>
/// registered by the host.
/// </para>
/// </remarks>
public class RemoteConnectionOptions : IEquatable<RemoteConnectionOptions> {

	/// <summary>
	/// Default constructor.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Uses the entry assembly's name as the application name.
	/// </para>
	/// </remarks>
	public RemoteConnectionOptions() {
		var asm = System.Reflection.Assembly.GetEntryAssembly();
		this.ApplicationName = asm?.GetName()?.Name ?? "RemoteConnection";
	}

	/// <summary>
	/// Constructor accepting the calling application's name.
	/// </summary>
	/// <param name="applicationName">The name of the client application opening the connection.</param>
	public RemoteConnectionOptions(string applicationName) {
		this.ApplicationName = applicationName;
	}

	/// <summary>
	/// Constructor accepting the calling application's name and the remote endpoint.
	/// </summary>
	/// <param name="applicationName">The name of the client application opening the connection.</param>
	/// <param name="endpointUri">The absolute Uri of the remote endpoint.</param>
	public RemoteConnectionOptions(string applicationName, Uri endpointUri) {
		this.ApplicationName = applicationName;
		this.EndpointUri = endpointUri;
	}

	/// <summary>
	/// Gets or sets the name of the client application opening the connection.
	/// </summary>
	/// <remarks>
	/// If provided, will be forwarded in the <see cref="RemoteIdentityConstants.AppNameHeader"/>
	/// (X-Cirreum-App-Name) header on the connection handshake.
	/// </remarks>
	public string ApplicationName { get; set; } = "";

	/// <summary>
	/// Gets or sets the Uri of the remote endpoint.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This should be an absolute uri.
	/// </para>
	/// </remarks>
	public Uri EndpointUri { get; set; } = new("", UriKind.RelativeOrAbsolute);

	/// <summary>
	/// Gets or sets the optional Authorization Header scheme and value.
	/// </summary>
	/// <remarks>
	/// Use <see cref="AuthorizationHeaderSettings.None"/> to connect without credentials.
	/// </remarks>
	public AuthorizationHeaderSettings? AuthorizationHeader { get; set; }

	/// <summary>
	/// Gets or sets an optional callback that supplies the credential for this connection.
	/// </summary>
	/// <remarks>
	/// Invoked before every connect and reconnect attempt. Return
	/// <see cref="AuthorizationHeaderSettings.None"/> to connect without a credential, or
	/// <see langword="null"/> when none is available.
	/// </remarks>
	public Func<CancellationToken, ValueTask<AuthorizationHeaderSettings?>>? CredentialProvider { get; set; }

	/// <summary>
	/// Gets or sets the scopes the connection's credential is requested for.
	/// </summary>
	/// <remarks>
	/// Read by the ambient <see cref="IRemoteConnectionCredentialSource"/>. A host that cannot
	/// determine an audience without them declines to supply a token rather than presenting one
	/// for an audience the application did not name.
	/// </remarks>
	public IReadOnlyList<string> Scopes { get; set; } = [];

	/// <summary>
	/// Gets or sets whether the connection re-establishes itself automatically after an
	/// unexpected close. Default: <see langword="true"/>.
	/// </summary>
	public bool Reconnect { get; set; } = true;

	/// <summary>
	/// Gets or sets the ceiling for the delay between reconnect attempts. Default: 30 seconds.
	/// </summary>
	public TimeSpan ReconnectMaxDelay { get; set; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// Determines whether the specified <see cref="RemoteConnectionOptions"/> is equal to the current instance.
	/// </summary>
	/// <param name="other">The <see cref="RemoteConnectionOptions"/> to compare with the current instance.</param>
	/// <returns>
	/// <see langword="true"/> if the specified <see cref="RemoteConnectionOptions"/> is equal to the current instance;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	/// <remarks>
	/// Two instances are considered equal if all their properties are equal.
	/// <see cref="CredentialProvider"/> is compared by delegate equality.
	/// </remarks>
	public bool Equals(RemoteConnectionOptions? other) {
		if (other is null) {
			return false;
		}

		if (ReferenceEquals(this, other)) {
			return true;
		}

		return this.ApplicationName == other.ApplicationName &&
			   this.EndpointUri.Equals(other.EndpointUri) &&
			   Equals(this.AuthorizationHeader, other.AuthorizationHeader) &&
			   Equals(this.CredentialProvider, other.CredentialProvider) &&
			   this.Scopes.SequenceEqual(other.Scopes) &&
			   this.Reconnect == other.Reconnect &&
			   this.ReconnectMaxDelay == other.ReconnectMaxDelay;
	}

	/// <summary>
	/// Determines whether the specified object is equal to the current instance.
	/// </summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns>
	/// <see langword="true"/> if the specified object is equal to the current instance;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	public override bool Equals(object? obj) {
		return this.Equals(obj as RemoteConnectionOptions);
	}

	/// <summary>
	/// Returns a hash code for the current instance.
	/// </summary>
	/// <returns>
	/// A hash code for the current instance, suitable for use in hashing algorithms and data structures
	/// like a hash table.
	/// </returns>
	public override int GetHashCode() {
		var hash = new HashCode();
		hash.Add(this.ApplicationName);
		hash.Add(this.EndpointUri);
		hash.Add(this.AuthorizationHeader);
		hash.Add(this.CredentialProvider);
		foreach (var scope in this.Scopes) {
			hash.Add(scope);
		}
		hash.Add(this.Reconnect);
		hash.Add(this.ReconnectMaxDelay);

		return hash.ToHashCode();
	}

	/// <summary>
	/// Determines whether two specified <see cref="RemoteConnectionOptions"/> instances are equal.
	/// </summary>
	/// <param name="left">The first <see cref="RemoteConnectionOptions"/> to compare.</param>
	/// <param name="right">The second <see cref="RemoteConnectionOptions"/> to compare.</param>
	/// <returns>
	/// <see langword="true"/> if the two instances are equal; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool operator ==(RemoteConnectionOptions? left, RemoteConnectionOptions? right) {
		return Equals(left, right);
	}

	/// <summary>
	/// Determines whether two specified <see cref="RemoteConnectionOptions"/> instances are not equal.
	/// </summary>
	/// <param name="left">The first <see cref="RemoteConnectionOptions"/> to compare.</param>
	/// <param name="right">The second <see cref="RemoteConnectionOptions"/> to compare.</param>
	/// <returns>
	/// <see langword="true"/> if the two instances are not equal; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool operator !=(RemoteConnectionOptions? left, RemoteConnectionOptions? right) {
		return !Equals(left, right);
	}

}
