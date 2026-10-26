namespace Cirreum.Invocation;

/// <summary>
/// A host invocation established by <see cref="IHostInvocationFactory.Begin"/>. Disposing it
/// ends the invocation: the ambient <see cref="IInvocationContextAccessor"/> is cleared and
/// the invocation's service scope is disposed.
/// </summary>
public interface IHostInvocation : IAsyncDisposable {

	/// <summary>
	/// The invocation context established for this host invocation.
	/// </summary>
	IInvocationContext Context { get; }

}
