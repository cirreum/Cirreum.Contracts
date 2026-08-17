namespace Cirreum.Invocation;

using Cirreum.Authentication;

/// <summary>
/// The authentication read surface of <see cref="IInvocationContext"/> — extension members
/// answering how the current invocation was authenticated, uniformly for every invocation
/// source.
/// </summary>
public static class InvocationContextExtensions {

	extension(IInvocationContext invocation) {

		/// <summary>
		/// Gets the authentication scheme that authenticated the current invocation, or
		/// <see langword="null"/> when the invocation did not flow through Cirreum scheme
		/// dispatch.
		/// </summary>
		/// <remarks>
		/// Reads the scheme stamp from <see cref="IInvocationContext.Items"/> — stamped by
		/// the dynamic forward selector for HTTP invocations, and seeded from the
		/// connection's authentication slots for long-lived sources. Describes how the
		/// invocation's transport was authenticated, not how its subject was established.
		/// </remarks>
		public string? AuthenticatedScheme =>
			invocation.Items.TryGetValue(AuthenticationContextKeys.AuthenticatedScheme, out var scheme)
				&& scheme is string value
					? value
					: null;

	}

}
