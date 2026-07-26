namespace Cirreum.Conductor;

/// <summary>
/// Publishes domain events to registered handlers.
/// </summary>
public interface IPublisher {
	/// <summary>
	/// Publishes a domain event to all registered handlers.
	/// </summary>
	/// <remarks>
	/// The publishing strategy is determined in the following priority order:
	/// <list type="number">
	/// <item><description>Explicit <paramref name="strategy"/> parameter (highest priority)</description></item>
	/// <item><description><see cref="PublishingStrategyAttribute"/> on the domain-event type</description></item>
	/// <item><description>Default strategy configured for this publisher instance (lowest priority)</description></item>
	/// </list>
	/// </remarks>
	/// <typeparam name="TDomainEvent">The type of domain event to publish.</typeparam>
	/// <param name="domainEvent">The domain event instance to publish.</param>
	/// <param name="strategy">
	/// Optional publishing strategy to use for this specific publication.
	/// If not specified, uses the strategy from <see cref="PublishingStrategyAttribute"/>
	/// or the publisher's default strategy.
	/// </param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>
	/// A <see cref="Task{TResult}"/> containing a <see cref="Result"/>
	/// indicating whether all handlers processed successfully.
	/// </returns>
	Task<Result> PublishAsync<TDomainEvent>(
		TDomainEvent domainEvent,
		PublisherStrategy? strategy = null,
		CancellationToken cancellationToken = default)
		where TDomainEvent : IDomainEvent;
}
