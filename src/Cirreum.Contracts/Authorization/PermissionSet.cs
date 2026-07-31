namespace Cirreum.Authorization;

using System.Collections;

/// <summary>
/// An immutable, ordered collection of <see cref="Permission"/> values with helpers for
/// membership tests, feature/operation queries, feature-scoped filtering, and deterministic
/// signature generation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PermissionSet"/> is general-purpose — it is not tied to grants, ACLs, or any
/// specific authorization stage. The grant pipeline uses it as the runtime representation of
/// <see cref="RequiresGrantAttribute"/> declarations (built once per type by
/// <see cref="RequiredGrantCache"/> and exposed via
/// <see cref="AuthorizationContext{TAuthorizableObject}.RequiredGrants"/>), but any code may
/// construct or consume a set for its own purposes.
/// </para>
/// <para>
/// Equality between elements follows <see cref="Permission"/> record equality — case-insensitive
/// on both <see cref="Permission.Feature"/> and <see cref="Permission.Operation"/>. The set is
/// sealed and fully immutable after construction.
/// </para>
/// </remarks>
/// <remarks>
/// Constructs a set from the supplied permissions. The items are copied into an internal
/// array; the set is immutable thereafter. Callers that need deduplication or validation
/// (e.g. AND-semantics across stacked attributes) perform it before construction.
/// </remarks>
/// <param name="items">The permissions to include.</param>
public sealed class PermissionSet(IReadOnlyList<Permission> items) : IReadOnlyList<Permission> {

	/// <summary>
	/// Shared empty set. Returned wherever a query or filter produces no results, so callers
	/// can compare against this instance without allocating.
	/// </summary>
	public static readonly PermissionSet Empty = new([]);

	private readonly Permission[] _items = items as Permission[] ?? [.. items];

	/// <inheritdoc />
	public int Count => this._items.Length;

	/// <summary>
	/// <see langword="true"/> when the set contains no permissions.
	/// </summary>
	public bool IsEmpty => this._items.Length == 0;

	/// <inheritdoc />
	public Permission this[int index] => this._items[index];

	/// <inheritdoc />
	public IEnumerator<Permission> GetEnumerator() =>
		((IEnumerable<Permission>)this._items).GetEnumerator();

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => this._items.GetEnumerator();

	/// <summary>
	/// <see langword="true"/> if the set contains <paramref name="permission"/>.
	/// </summary>
	public bool Contains(Permission permission) {
		for (var i = 0; i < this._items.Length; i++) {
			if (this._items[i].Equals(permission)) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// <see langword="true"/> if the set contains the permission expressed in
	/// <c>"feature:operation"</c> format (e.g., <c>"issues:delete"</c>). The string is parsed
	/// via <see cref="Permission.Parse"/>.
	/// </summary>
	/// <exception cref="FormatException">
	/// <paramref name="featureAndOperation"/> is not in <c>"feature:operation"</c> form.
	/// </exception>
	public bool Contains(string featureAndOperation) =>
		this.Contains(Permission.Parse(featureAndOperation));

	/// <summary>
	/// <see langword="true"/> if the set contains at least one of <paramref name="permissions"/>.
	/// </summary>
	public bool ContainsAny(params Permission[] permissions) {
		for (var i = 0; i < permissions.Length; i++) {
			if (this.Contains(permissions[i])) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// <see langword="true"/> if the set contains every one of <paramref name="permissions"/>.
	/// </summary>
	public bool ContainsAll(params Permission[] permissions) {
		for (var i = 0; i < permissions.Length; i++) {
			if (!this.Contains(permissions[i])) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// <see langword="true"/> if any permission in the set belongs to the given feature area
	/// (e.g., <c>"issues"</c>). Comparison is case-insensitive.
	/// </summary>
	public bool HasFeature(string feature) {
		for (var i = 0; i < this._items.Length; i++) {
			if (string.Equals(this._items[i].Feature, feature, StringComparison.OrdinalIgnoreCase)) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// <see langword="true"/> if any permission in the set has the given operation verb
	/// (e.g., <c>"delete"</c>), regardless of feature. Comparison is case-insensitive.
	/// </summary>
	public bool HasOperation(string operation) {
		for (var i = 0; i < this._items.Length; i++) {
			if (string.Equals(this._items[i].Operation, operation, StringComparison.OrdinalIgnoreCase)) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// <see langword="true"/> when <paramref name="heldEntries"/> satisfies every permission
	/// in this set (AND semantics). An entry satisfies a required permission when it parses
	/// as <c>feature:operation</c> and equals it (case-insensitive), or — only when
	/// <paramref name="allowBareActionShorthand"/> is <see langword="true"/> — when it is a
	/// bare action (no <c>:</c>) equal to the required operation.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the canonical grant-entry matcher: grant providers should call it instead of
	/// hand-rolling string comparisons, so matching semantics cannot drift per app.
	/// </para>
	/// <para>
	/// <b>Bare-action shorthand is a cross-feature wildcard.</b> With the flag enabled, a held
	/// entry of <c>"delete"</c> satisfies <c>anything:delete</c> — treat bare-action entries as
	/// privileged and enable the flag only when the app's grant vocabulary deliberately uses
	/// them. Blank, whitespace, and malformed entries (e.g., <c>"loans:"</c>) never match
	/// anything. An empty required set is satisfied by any input (vacuous AND).
	/// </para>
	/// </remarks>
	public bool IsSatisfiedBy(IEnumerable<string> heldEntries, bool allowBareActionShorthand = false) {
		ArgumentNullException.ThrowIfNull(heldEntries);

		if (this._items.Length == 0) {
			return true;
		}

		List<Permission>? held = null;
		List<string>? bareActions = null;
		foreach (var entry in heldEntries) {
			if (string.IsNullOrWhiteSpace(entry)) {
				continue;
			}
			if (Permission.TryParse(entry, out var parsed)) {
				(held ??= []).Add(parsed!);
			} else if (allowBareActionShorthand && !entry.Contains(':')) {
				(bareActions ??= []).Add(entry.Trim());
			}
		}

		for (var i = 0; i < this._items.Length; i++) {
			if (!IsRequirementMet(this._items[i], held, bareActions)) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// <see langword="true"/> when <paramref name="heldPermissions"/> contains every permission
	/// in this set (AND semantics, case-insensitive). The already-parsed counterpart of
	/// <see cref="IsSatisfiedBy(IEnumerable{string}, bool)"/>; bare-action shorthand does not
	/// apply because a <see cref="Permission"/> always carries a feature.
	/// </summary>
	public bool IsSatisfiedBy(IEnumerable<Permission> heldPermissions) {
		ArgumentNullException.ThrowIfNull(heldPermissions);

		if (this._items.Length == 0) {
			return true;
		}

		var held = heldPermissions as IReadOnlyCollection<Permission> ?? [.. heldPermissions];
		for (var i = 0; i < this._items.Length; i++) {
			var required = this._items[i];
			var met = false;
			foreach (var candidate in held) {
				if (required.Equals(candidate)) {
					met = true;
					break;
				}
			}
			if (!met) {
				return false;
			}
		}
		return true;
	}

	private static bool IsRequirementMet(
		Permission required,
		List<Permission>? held,
		List<string>? bareActions) {

		if (held is not null) {
			for (var i = 0; i < held.Count; i++) {
				if (required.Equals(held[i])) {
					return true;
				}
			}
		}
		if (bareActions is not null) {
			for (var i = 0; i < bareActions.Count; i++) {
				if (string.Equals(bareActions[i], required.Operation, StringComparison.OrdinalIgnoreCase)) {
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Returns the subset of permissions belonging to <paramref name="feature"/>, or
	/// <see cref="Empty"/> if none match. Comparison is case-insensitive.
	/// </summary>
	public PermissionSet ForFeature(string feature) {
		List<Permission>? filtered = null;
		for (var i = 0; i < this._items.Length; i++) {
			if (string.Equals(this._items[i].Feature, feature, StringComparison.OrdinalIgnoreCase)) {
				(filtered ??= []).Add(this._items[i]);
			}
		}
		return filtered is null ? Empty : new PermissionSet(filtered);
	}

}
