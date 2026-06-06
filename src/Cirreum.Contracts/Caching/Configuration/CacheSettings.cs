namespace Cirreum.Caching.Configuration;

/// <summary>
/// Central cache configuration for the Cirreum platform. Bound from the
/// <c>Cirreum:Cache</c> configuration section.
/// </summary>
/// <remarks>
/// Holds the default expiration policy applied to all cache participants unless they override it.
/// The active cache implementation is chosen <em>in code</em> — the no-op default, or a provider's
/// <c>Add*CacheService</c> registration — not via configuration. Subsystems are cache
/// <em>participants</em>: they may override expiration per-category or per-domain but do not choose the
/// provider or global defaults independently.
/// </remarks>
public sealed class CacheSettings {

	/// <summary>
	/// The configuration section path for binding.
	/// </summary>
	public const string SectionPath = "Cirreum:Cache";

	/// <summary>
	/// Default expiration policy inherited by all cache participants unless they
	/// specify an override at the category, query, or domain level.
	/// </summary>
	/// <example>
	/// <code>
	/// "DefaultExpiration": {
	///   "Expiration": "00:05:00",
	///   "LocalExpiration": "00:02:00",
	///   "FailureExpiration": "00:00:30"
	/// }
	/// </code>
	/// </example>
	public CacheExpirationOverride DefaultExpiration { get; set; } = new();
}
