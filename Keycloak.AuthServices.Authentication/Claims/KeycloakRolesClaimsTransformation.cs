namespace Keycloak.AuthServices.Authentication.Claims;

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Transforms keycloak roles in the resource_access claim to jwt role claims.
/// Note, realm roles are not mapped atm.
/// </summary>
/// <example>
/// Example of keycloack resource_access claim
/// "resource_access": {
///     "api": {
///         "roles": [ "role1", "role2" ]
///     },
///     "account": {
///         "roles": [
///             "view-profile"
///         ]
///     }
/// },
/// </example>
/// <seealso cref="IClaimsTransformation" />
public class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    private readonly string roleClaimType;
    private readonly string audience;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeycloakRolesClaimsTransformation"/> class.
    /// </summary>
    /// <param name="roleClaimType">Type of the role claim.</param>
    /// <param name="audience">The audience.</param>
    public KeycloakRolesClaimsTransformation(string roleClaimType, string audience)
    {
        this.roleClaimType = roleClaimType;
        this.audience = audience;
    }

    /// <summary>
    /// Provides a central transformation point to change the specified principal.
    /// Note: this will be run on each AuthenticateAsync call, so its safer to
    /// return a new ClaimsPrincipal if your transformation is not idempotent.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal" /> to transform.</param>
    /// <returns>
    /// The transformed principal.
    /// </returns>
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var result = principal.Clone();
        if (result.Identity is not ClaimsIdentity identity)
        {
            return Task.FromResult(result);
        }

        //var resourceAccessValue = principal.FindFirst("resource_access")?.Value;
        var resourceAccessValue = principal.FindFirst("realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccessValue))
        {
            return Task.FromResult(result);
        }



        using var resourceAccess = JsonDocument.Parse(resourceAccessValue);

        var clientRoles = resourceAccess.RootElement.GetProperty("roles");

        foreach (var role in clientRoles.EnumerateArray())
        {
            var value = role.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                identity.AddClaim(new Claim(this.roleClaimType, value));
            }
        }


        //var containsAudienceRoles = resourceAccess
        //    .RootElement
        //    .TryGetProperty(this.audience, out var rolesElement);

        //if (!containsAudienceRoles)
        //{
        //    return Task.FromResult(result);
        //}

        //var clientRoles = rolesElement.GetProperty("roles");

        //foreach (var role in clientRoles.EnumerateArray())
        //{
        //    var value = role.GetString();
        //    if (!string.IsNullOrWhiteSpace(value))
        //    {
        //        identity.AddClaim(new Claim(this.roleClaimType, value));
        //    }
        //}

        return Task.FromResult(result);
    }
}
