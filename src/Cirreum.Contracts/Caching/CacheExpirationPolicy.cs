namespace Cirreum.Caching;

/// <summary>
/// The resolved expiration policy for a single cache operation — the absolute (L2), local (L1), and
/// failure expirations passed to <see cref="ICacheService"/> on each call. The framework builds this at
/// runtime from the configured defaults/overrides; it is <em>not</em> bound from configuration (the
/// config-time shape is <see cref="Cirreum.Caching.Configuration.CacheExpirationOverride"/>).
/// </summary>
/// <param name="Expiration">
/// The absolute expiration duration for the cache entry. Applied to distributed (L2)
/// cache when present. If <see langword="null"/>, the caching service's default is used.
/// </param>
/// <param name="LocalExpiration">
/// The absolute expiration duration for the local (L1) in-memory cache. If
/// <see langword="null"/>, falls back to <paramref name="Expiration"/>. Set shorter
/// than <paramref name="Expiration"/> to reduce memory pressure or ensure fresher
/// data locally while still benefiting from L2.
/// </param>
/// <param name="FailureExpiration">
/// The expiration duration for cache entries storing failed results (<see cref="Result"/>
/// with <c>IsSuccess = false</c>). Should be shorter than <paramref name="Expiration"/>
/// to avoid caching transient failures for too long. If <see langword="null"/>, uses
/// the standard <paramref name="Expiration"/>.
/// </param>
public sealed record CacheExpirationPolicy(
	TimeSpan? Expiration = null,
	TimeSpan? LocalExpiration = null,
	TimeSpan? FailureExpiration = null
);
