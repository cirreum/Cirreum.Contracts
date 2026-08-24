namespace Cirreum.Invocation.Connections;

/// <summary>
/// Endpoint metadata marking an endpoint as one whose invocations arrive over an
/// <see cref="IInvocationConnection"/>.
/// </summary>
/// <remarks>
/// <para>
/// Stamped by the framework on the connection endpoints it maps, and available to an
/// application mapping a transport of its own. Authentication consults it to decide whether a
/// query-carried bearer credential is acceptable on the request: a browser cannot set headers
/// on a WebSocket upgrade, so a credential there has nowhere else to travel.
/// </para>
/// <para>
/// A SignalR hub needs no stamp — it carries SignalR's own hub metadata, which authentication
/// recognizes directly.
/// </para>
/// <para>
/// Declared here, alongside the rest of the connection vocabulary and free of any framework
/// dependency, so that both the server spine that stamps it and the authentication schemes
/// that read it can reach it without either referencing the other.
/// </para>
/// </remarks>
public sealed class InvocationConnectionMetadata {

	/// <summary>The shared instance. The metadata carries no per-endpoint state.</summary>
	public static InvocationConnectionMetadata Instance { get; } = new();

	private InvocationConnectionMetadata() {
	}

}
