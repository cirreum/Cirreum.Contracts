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
		/// <para>
		/// Reads the scheme stamp from <see cref="IInvocationContext.Items"/> — stamped by
		/// the dynamic forward selector for HTTP invocations, and seeded from the connection's
		/// authentication slots for long-lived sources.
		/// </para>
		///<para>
		/// For long-lived sources, this value describes how the connection was authenticated
		/// at establishment. It does not change when the connection is later promoted through
		/// Two-Phase Auth and therefore does not necessarily describe how
		/// <see cref="IInvocationContext.User"/> was established.
		///</para>
		/// </remarks>
		public string? AuthenticatedScheme =>
			invocation.Items.TryGetValue(AuthenticationContextKeys.AuthenticatedScheme, out var scheme)
				&& scheme is string value
					? value
					: null;

	}

}
