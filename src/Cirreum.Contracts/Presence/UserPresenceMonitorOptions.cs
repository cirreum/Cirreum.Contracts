namespace Cirreum.Presence;

/// <summary>
/// Configuration options for user presence monitoring.
/// </summary>
public class UserPresenceMonitorOptions {

	/// <summary>
	/// The minimum enabled refresh interval, in milliseconds.
	/// </summary>
	/// <remarks>
	/// Positive refresh intervals must be greater than this value. A value of
	/// <c>0</c> is reserved for disabling presence monitoring.
	/// </remarks>
	public const int MinimumRefreshInterval = 5_000;

	/// <summary>
	/// The default refresh interval, in milliseconds.
	/// </summary>
	/// <remarks>
	/// The default interval is one minute.
	/// </remarks>
	public const int DefaultRefreshInterval = 60_000;

	/// <summary>
	/// Gets or sets the interval, in milliseconds, between presence updates.
	/// </summary>
	/// <value>
	/// Defaults to <see cref="DefaultRefreshInterval"/>. A value of <c>0</c> disables
	/// presence monitoring. Positive values must be greater than
	/// <see cref="MinimumRefreshInterval"/>; invalid values fall back to
	/// <see cref="DefaultRefreshInterval"/>.
	/// </value>
	/// <remarks>
	/// Smaller intervals provide more frequent presence updates but increase network and
	/// server activity. Larger intervals reduce that activity at the cost of less
	/// immediate presence information.
	/// </remarks>
	public int RefreshInterval { get; set; } = DefaultRefreshInterval;

}