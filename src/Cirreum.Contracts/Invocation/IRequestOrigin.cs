namespace Cirreum.Invocation;

/// <summary>
/// Describes the origin of a server-side invocation for audit, telemetry, and
/// origin-aware policy decisions.
/// </summary>
/// <remarks>
/// <para>
/// Materialized by the server-side <c>UserStateAccessor</c> and surfaced through
/// <c>IUserState.Origin</c>. Captures both the application-defined channel and the
/// framework invocation source so consumers can distinguish, for example, browser
/// WebSocket traffic from machine-to-machine HTTP even when multiple channels share
/// the same underlying source adapter.
/// </para>
/// <para>
/// Origin metadata is not proof of identity. Authorization continues to rely on the
/// authenticated subject, grants, roles, permissions, and other applicable policy.
/// Authorizers may use origin as an additional policy constraint, but should not treat
/// <see cref="Channel"/> or <see cref="InvocationSource"/> alone as authentication
/// evidence.
/// </para>
/// </remarks>
public interface IRequestOrigin {

	/// <summary>
	/// Gets the application-defined channel through which the invocation originated.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Describes the caller-facing or application-level path, such as
	/// <c>"WebChat"</c>, <c>"Twilio"</c>, <c>"PartnerWebhook"</c>, or
	/// <c>"OperatorPortal"</c>.
	/// </para>
	/// <para>
	/// This is distinct from <see cref="InvocationSource"/>, which identifies the
	/// framework adapter that materialized the invocation (for example,
	/// <see cref="InvocationSources.Http"/>, <see cref="InvocationSources.SignalR"/>,
	/// or <see cref="InvocationSources.WebSocket"/>).
	/// </para>
	/// </remarks>
	string Channel { get; }

	/// <summary>
	/// Gets an optional application-defined correlation reference associated with the
	/// invocation.
	/// </summary>
	/// <remarks>
	/// May contain a conversation identifier, webhook event identifier, request
	/// reference, or similar value. The framework carries the value for telemetry and
	/// audit without interpreting it. <see langword="null"/> when no reference was
	/// supplied.
	/// </remarks>
	string? Reference { get; }

	/// <summary>
	/// Gets the framework invocation source captured when this origin was materialized.
	/// </summary>
	/// <remarks>
	/// Mirrors <see cref="IInvocationContext.InvocationSource"/> so consumers holding only
	/// the origin can determine which framework adapter produced the invocation without
	/// resolving the ambient invocation context. Framework-known values are defined by
	/// <see cref="InvocationSources"/>.
	/// </remarks>
	string InvocationSource { get; }

}