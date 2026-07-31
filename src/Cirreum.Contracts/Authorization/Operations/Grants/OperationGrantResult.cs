namespace Cirreum.Authorization.Operations.Grants;

/// <summary>
/// The raw result of an app's grant-table lookup — owner IDs (and optional auxiliary
/// dimensions) where the caller holds ALL required permissions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="OperationGrantResult"/> is what an <see cref="IOperationGrantProvider"/> returns
/// from <c>ResolveGrantsAsync</c>. Core's orchestrator wraps it into a full <see cref="OperationGrant"/>
/// — collapsing an empty owner set to <see cref="OperationGrant.Denied"/> and passing
/// <see cref="Extensions"/> through to the gate/handler.
/// </para>
/// <para>
/// Apps never construct an <see cref="OperationGrant"/> directly; they construct <see cref="OperationGrantResult"/>
/// and let the orchestrator apply translation policy.
/// </para>
/// </remarks>
/// <param name="OwnerIds">
/// The owner IDs the caller has been granted all required permissions on. May be empty
/// (no qualifying grants) — the orchestrator collapses an empty owner set to
/// <see cref="OperationGrant.Denied"/>.
/// </param>
/// <param name="Extensions">
/// Optional app-specific auxiliary dimensions (e.g., SSV codes, tiers, regions) attached to
/// the grants. Opaque to Core — handlers read these via <c>IOperationGrantAccessor</c> and apply
/// them as additional predicate scope.
/// </param>
public sealed record OperationGrantResult(
	IReadOnlyList<string> OwnerIds,
	IReadOnlyDictionary<string, object>? Extensions = null);
