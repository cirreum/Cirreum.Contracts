namespace Cirreum.Invocation;

using Cirreum.Authentication;

/// <summary>
/// The authentication read surface of <see cref="IInvocationContext"/> — extension members
/// describing how the invocation and its effective subject were authenticated, uniformly
/// across invocation sources.
/// </summary>
public static class InvocationContextExtensions {

	extension(IInvocationContext invocation) {

		/// <summary>
		/// Gets the scheme that authenticated the invocation's transport, or
		/// <see langword="null"/> when the invocation did not flow through Cirreum scheme
		/// dispatch.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Reads the scheme stamp from <see cref="IInvocationContext.Items"/> — stamped by
		/// the dynamic forward selector for stateless invocations and seeded from the
		/// connection's authentication state for long-lived sources.
		/// </para>
		/// <para>
		/// For long-lived sources, this describes how the connection was authenticated at
		/// establishment. It does not change when the connection is later promoted through
		/// Two-Phase Auth and therefore may differ from the scheme that established
		/// <see cref="IInvocationContext.User"/>.
		/// </para>
		/// </remarks>
		public string? AuthenticatedScheme =>
			ReadScheme(invocation.Items, AuthenticationContextKeys.AuthenticatedScheme);

		/// <summary>
		/// Gets the scheme that established the invocation's effective subject when that
		/// scheme differs from <see cref="extension(IInvocationContext).AuthenticatedScheme"/>, or
		/// <see langword="null"/> when the transport authentication scheme also established
		/// the subject.
		/// </summary>
		/// <remarks>
		/// For long-lived sources, this value is snapshotted from the connection when the
		/// invocation is created. Promotion during the current invocation therefore becomes
		/// visible beginning with the next invocation, alongside the promoted
		/// <see cref="IInvocationContext.User"/>.
		/// </remarks>
		public string? OriginScheme =>
			ReadScheme(invocation.Items, AuthenticationContextKeys.OriginScheme);

		/// <summary>
		/// Gets the scheme whose authentication declarations apply to the invocation's
		/// effective subject — <see cref="extension(IInvocationContext).OriginScheme"/> when present, otherwise
		/// <see cref="extension(IInvocationContext).AuthenticatedScheme"/>.
		/// </summary>
		public string? EffectiveScheme =>
			invocation.OriginScheme ?? invocation.AuthenticatedScheme;

	}

	private static string? ReadScheme(
		IDictionary<object, object?>? items,
		string key) =>
		items is not null
			&& items.TryGetValue(key, out var value)
			&& value is string scheme
				? scheme
				: null;

}
