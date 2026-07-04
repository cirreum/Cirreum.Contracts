namespace Cirreum;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Interface for configuring user profile enrichment services.
/// </summary>
public interface IUserProfileEnrichmentBuilder {

	/// <summary>
	/// Gets the service collection.
	/// </summary>
	IServiceCollection Services { get; }

}
