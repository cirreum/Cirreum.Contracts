namespace Cirreum.Invocation;

/// <summary>
/// Establishes an <see cref="IInvocationContext"/> for work the host starts on its own
/// behalf: a hosted service loop, a startup task, a received message, or work continued
/// after the inbound invocation that requested it has completed.
/// </summary>
/// <remarks>
/// <para>
/// A host invocation has its own service scope, a cancellation token tied to application
/// shutdown, no connection, and — unless <see cref="HostInvocationOptions.User"/> supplies
/// one — a principal that names the host application on the
/// <see cref="HostInvocationDefaults.Scheme"/> scheme.
/// </para>
/// <para>
/// A host invocation never nests inside another invocation. <see cref="Begin"/> throws when
/// <see cref="IInvocationContextAccessor.Current"/> is already set. Work continued from an
/// inbound invocation is detached from that invocation's execution context first, either
/// with <see cref="ExecutionContext.SuppressFlow"/> or through
/// <see cref="HostInvocationFactoryExtensions.extension(IHostInvocationFactory).StartDetached"/>.
/// </para>
/// </remarks>
public interface IHostInvocationFactory {

	/// <summary>
	/// Begins a host invocation and sets it as the current invocation for the calling async
	/// flow.
	/// </summary>
	/// <param name="options">The invocation's source, subject, roles, and cancellation. When
	/// <see langword="null"/>, the defaults apply.</param>
	/// <returns>The invocation. Dispose it to end the invocation.</returns>
	/// <exception cref="InvalidOperationException">An invocation is already current on the
	/// calling async flow, or <see cref="HostInvocationOptions.User"/> is not authenticated.</exception>
	IHostInvocation Begin(HostInvocationOptions? options = null);

}
