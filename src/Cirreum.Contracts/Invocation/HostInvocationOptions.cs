namespace Cirreum.Invocation;

using System.Security.Claims;

/// <summary>
/// Options for one host invocation begun through <see cref="IHostInvocationFactory.Begin"/>.
/// </summary>
public sealed class HostInvocationOptions {

	/// <summary>
	/// The value of <see cref="IInvocationContext.InvocationSource"/> for the invocation.
	/// Defaults to <see cref="InvocationSources.Host"/>.
	/// </summary>
	public string Source { get; init; } = InvocationSources.Host;

	/// <summary>
	/// The subject the work runs as. When <see langword="null"/>, the invocation runs as the
	/// host application. When supplied, the principal must be authenticated.
	/// </summary>
	/// <remarks>
	/// The supplied principal is not modified. When <see cref="Roles"/> are also supplied, the
	/// invocation's principal is a new <see cref="ClaimsPrincipal"/> over the supplied
	/// identities plus one identity on the <see cref="HostInvocationDefaults.Scheme"/> scheme
	/// carrying the role claims.
	/// </remarks>
	public ClaimsPrincipal? User { get; init; }

	/// <summary>
	/// The scheme that established <see cref="User"/>, stamped as the invocation's origin
	/// scheme. Ignored when <see cref="User"/> is <see langword="null"/>.
	/// </summary>
	public string? OriginScheme { get; init; }

	/// <summary>
	/// The roles the work runs with, minted as role claims on the invocation's principal.
	/// When <see langword="null"/>, the host's configured default roles apply; an empty list
	/// mints none.
	/// </summary>
	public IReadOnlyList<string>? Roles { get; init; }

	/// <summary>
	/// A caller-supplied token linked into the invocation's
	/// <see cref="IInvocationContext.Aborted"/> alongside application shutdown.
	/// </summary>
	public CancellationToken CancellationToken { get; init; }

}
