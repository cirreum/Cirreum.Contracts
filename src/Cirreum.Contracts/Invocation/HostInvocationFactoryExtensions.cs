namespace Cirreum.Invocation;

/// <summary>
/// Extension members over <see cref="IHostInvocationFactory"/>.
/// </summary>
public static class HostInvocationFactoryExtensions {

	extension(IHostInvocationFactory factory) {

		/// <summary>
		/// Runs <paramref name="work"/> on the thread pool inside a host invocation, detached
		/// from the caller's execution context.
		/// </summary>
		/// <remarks>
		/// Suppresses <see cref="ExecutionContext"/> flow before queuing, so an invocation
		/// current on the caller does not flow into the work, then begins a host invocation
		/// on the pool thread and ends it when <paramref name="work"/> completes. The returned
		/// task completes with the work's outcome; exceptions are not observed here.
		/// </remarks>
		/// <param name="work">The work to run. Receives the host invocation's context.</param>
		/// <param name="options">Options for the host invocation, or <see langword="null"/>
		/// for the defaults.</param>
		/// <returns>A task that completes when <paramref name="work"/> completes.</returns>
		public Task StartDetached(
			Func<IInvocationContext, Task> work,
			HostInvocationOptions? options = null) {

			ArgumentNullException.ThrowIfNull(factory);
			ArgumentNullException.ThrowIfNull(work);

			using (ExecutionContext.SuppressFlow()) {
				return Task.Run(async () => {
					await using var invocation = factory.Begin(options);
					await work(invocation.Context);
				});
			}

		}

	}

}
