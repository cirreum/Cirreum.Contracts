namespace Cirreum.Invocation;

/// <summary>
/// Typed annotation describing where a server-side request originated. Replaces the
/// deprecated <c>IActorContext</c> surface with a server-only,
/// transport-aware origin model.
/// </summary>
/// <remarks>
/// <para>
/// Surfaced on the framework's <c>IUserState.Origin</c> property by the server-side
/// <c>UserStateAccessor</c> at <see cref="IInvocationContext"/> materialization. Used for
/// audit, telemetry, and origin-aware authorization where the *transport channel* itself
/// is part of the access policy (e.g., "this operation only over WebSocket promoted with
/// Two-Phase Auth", "this operation only via direct HTTP from operator IdP").
/// </para>
/// <para>
/// <b>Not security-load-bearing.</b> Origin is a diagnostic + audit shape; authorization
/// decisions still flow through grants + roles + permissions. Authorizers may *read* origin
/// for policy gating but should not treat the channel alone as proof of identity.
/// </para>
/// </remarks>
public interface IRequestOrigin {

	/// <summary>
	/// The transport channel that delivered the request — a stable string identifier
	/// from a small known set (e.g., <c>"Http"</c>, <c>"WebSocket"</c>, <c>"SignalR"</c>,
	/// <c>"gRPC"</c>, <c>"Webhook"</c>, <c>"BackgroundTimer"</c>, <c>"QueueListener"</c>).
	/// Distinct from <see cref="InvocationSource"/> in that Channel categorizes the
	/// caller's perspective (browser-WS, M2M-HTTP, etc.) while InvocationSource names the
	/// framework adapter that materialized the invocation.
	/// </summary>
	string Channel { get; }

	/// <summary>
	/// Optional caller-supplied correlation reference — request id, conversation id,
	/// webhook event id, etc. Carried through telemetry and audit without interpretation
	/// by the framework. <see langword="null"/> when no reference is provided.
	/// </summary>
	string? Reference { get; }

	/// <summary>
	/// Denormalized <see cref="IInvocationContext.InvocationSource"/> at the time the
	/// origin was captured — duplicated here so origin-aware consumers can reason about
	/// the source without re-resolving the invocation context. Matches
	/// <see cref="InvocationSources"/> values.
	/// </summary>
	string InvocationSource { get; }

}
