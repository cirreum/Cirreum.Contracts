namespace Cirreum.Authorization;

/// <summary>
/// Defines an authorizer that enforces authorization rules for a specific <see cref="IAuthorizableObject"/> type.
/// </summary>
/// <typeparam name="TAuthorizableObject">The type of <see cref="IAuthorizableObject"/> to authorize.</typeparam>
/// <remarks>
/// <para>
/// Authorizers enforce permissions by evaluating user roles, state, and object properties.
/// While commonly used with commands/queries, authorizers can be used with any type that
/// implements <see cref="IAuthorizableObject"/>.
/// </para>
/// <para>
/// This flexible design allows for Attribute-Based Access Control (ABAC) to be enforced anywhere
/// in the application, not just within the Conductor pipeline.
/// </para>
/// <para>
/// Most implementations inherit a framework-provided base class rather than
/// implementing this interface directly.
/// </para>
/// <para>
/// Works with commands, queries, domain entities, or any type that implements
/// <see cref="IAuthorizableObject"/>.
/// </para>
/// </remarks>
public interface IAuthorizer<TAuthorizableObject>
	where TAuthorizableObject : IAuthorizableObject;
