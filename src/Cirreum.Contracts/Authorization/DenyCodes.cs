namespace Cirreum.Authorization;

/// <summary>
/// Stable machine-readable codes for authorization denials.
/// Emitted on telemetry (<c>cirreum.authz.reason</c>).
/// </summary>
public static class DenyCodes {

	/// <summary>
	/// Caller is not authenticated. Emitted by the preflight authentication gate.
	/// </summary>
	public const string AuthenticationRequired = "AUTHENTICATION_REQUIRED";

	/// <summary>
	/// Caller's application user is disabled. Emitted by the preflight application-user gate
	/// when a resolved application user reports <see cref="IApplicationUser.IsEnabled"/> of
	/// <see langword="false"/>. Callers with no application-user record are not subject to it.
	/// </summary>
	public const string UserDisabled = "USER_DISABLED";

	/// <summary>
	/// Caller holds no roles registered with the application's role registry.
	/// Emitted by the preflight role gate.
	/// </summary>
	public const string NoRolesAssigned = "NO_ROLES_ASSIGNED";

	/// <summary>
	/// The authorizable object has no authorizer, grant surface, constraint, or applicable
	/// policy — nothing would evaluate it. Emitted by the preflight authorizer-presence gate,
	/// which denies fail-closed rather than admitting an unguarded operation. A
	/// misconfiguration rather than an access decision.
	/// </summary>
	public const string NoAuthorizersRegistered = "NO_AUTHORIZERS_REGISTERED";

	/// <summary>
	/// OwnerId is required on the resource but was not supplied. Emitted on Global-scope
	/// write attempts that did not name a target tenant.
	/// </summary>
	public const string OwnerIdRequired = "OWNER_ID_REQUIRED";

	/// <summary>
	/// OwnerId is required for cacheable owner-scoped reads but was not supplied.
	/// Emitted on Global-scope reads of cacheable grant-aware lookups
	/// that did not name a target tenant — required to keep the cache keyed per-tenant.
	/// </summary>
	public const string CacheableReadOwnerIdRequired = "CACHEABLE_READ_OWNER_ID_REQUIRED";

	/// <summary>
	/// Resource OwnerId does not match the caller's tenant.
	/// </summary>
	public const string OwnerIdMismatch = "OWNER_ID_MISMATCH";

	/// <summary>
	/// Caller's access scope is not permitted for this resource.
	/// </summary>
	public const string ScopeNotPermitted = "SCOPE_NOT_PERMITTED";

	/// <summary>
	/// The requested owner is not in the caller's reach for this operation.
	/// Emitted when a command's <c>OwnerId</c> or a list's <c>OwnerIds</c> contains an
	/// owner the caller has no grant for.
	/// </summary>
	public const string OwnerNotInReach = "OWNER_NOT_IN_REACH";

	/// <summary>
	/// The caller's granted access is empty for this operation (no qualifying grant records).
	/// Emitted when a command or list cannot be stamped/enriched because the caller has
	/// no granted access.
	/// </summary>
	public const string GrantDenied = "GRANT_DENIED";

	/// <summary>
	/// The caller's reach is ambiguous for enrichment: more than one owner is reachable
	/// and the request did not name one. Emitted on Tenant-scope commands/reads with null
	/// OwnerId and a multi-element reach.
	/// </summary>
	public const string OwnerAmbiguous = "OWNER_AMBIGUOUS";

	/// <summary>
	/// Id is required on a self-scoped resource but was not supplied or is invalid. Emitted on
	/// <see cref="Operations.Grants.IGrantableSelfBase"/> operations with a null or invalid <c>Id</c>.
	/// </summary>
	public const string ResourceIdRequired = "RESOURCE_ID_REQUIRED";

	/// <summary>
	/// The resource's ExternalId does not match the calling user and the caller does not
	/// have bypass (admin) privileges. Emitted on <see cref="Operations.Grants.IGrantableSelfBase"/>
	/// operations where the identity match fails.
	/// </summary>
	public const string NotResourceOwner = "NOT_RESOURCE_OWNER";

	/// <summary>
	/// The caller does not have the required permission on the protected resource.
	/// Emitted by <see cref="Resources.IResourceAccessEvaluator"/> when the resource's
	/// effective ACL does not grant the requested permission for any of the caller's roles.
	/// </summary>
	public const string ResourceAccessDenied = "RESOURCE_ACCESS_DENIED";

	/// <summary>
	/// The protected resource was not found by its identifier.
	/// Emitted by <see cref="Resources.IResourceAccessEvaluator"/> when
	/// <see cref="Resources.IAccessEntryProvider{T}.GetByIdAsync"/> returns <see langword="null"/>.
	/// </summary>
	public const string ResourceNotFound = "RESOURCE_NOT_FOUND";

	/// <summary>
	/// An unexpected exception was thrown while evaluating authorization. The pipeline denies
	/// rather than admitting a caller whose evaluation did not complete.
	/// </summary>
	public const string EvaluationError = "EVALUATION_ERROR";

	/// <summary>
	/// A denial whose evaluator supplied no code. Emitted when a constraint, object authorizer,
	/// or policy authorizer returns a failure with no <c>ErrorCode</c> set — the reason is
	/// unknown to the pipeline, not absent.
	/// </summary>
	public const string Unknown = "UNKNOWN";
}
