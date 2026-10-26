namespace Cirreum.Invocation;

/// <summary>
/// Framework-known values for host invocations.
/// </summary>
public static class HostInvocationDefaults {

	/// <summary>
	/// The authentication scheme name stamped on every host invocation and declared as a
	/// machine scheme. The host principal's <c>AuthenticationType</c> is this value.
	/// </summary>
	public const string Scheme = "Cirreum.Host";

}
