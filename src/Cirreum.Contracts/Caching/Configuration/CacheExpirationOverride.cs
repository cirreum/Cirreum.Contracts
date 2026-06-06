namespace Cirreum.Caching.Configuration;

/// <summary>
/// A config-bound cache-expiration override — the absolute (L2), local (L1), and failure expirations.
/// Any field left <see langword="null"/> falls through to the global default
/// (<see cref="CacheSettings.DefaultExpiration"/>). Reused as the global default and as the value type
/// for per-consumer override maps (e.g. the Conductor's per-query overrides). The runtime, resolved
/// counterpart handed to <c>ICacheService</c> is <see cref="CacheExpirationPolicy"/>.
/// </summary>
public class CacheExpirationOverride {

	/// <summary>
	/// Override for distributed (L2) cache expiration.
	/// </summary>
	public TimeSpan? Expiration { get; set; }

	/// <summary>
	/// Override for local (L1) in-memory cache expiration.
	/// </summary>
	public TimeSpan? LocalExpiration { get; set; }

	/// <summary>
	/// Override for failure expiration.
	/// </summary>
	public TimeSpan? FailureExpiration { get; set; }
}
