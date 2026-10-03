using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Samples.Models;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Security.Claims;
using static Duende.IdentityModel.OidcConstants;

namespace Bacon.ApiUtilities.Samples.Attributes.Authorization;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BaseCustomAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter, IEndpointAccessPrivileges<Privileges>
{
    #region Properties

    /// <summary>
    /// 
    /// </summary>
    public IList<Privileges> Privileges { get; private set; } = [];

    private readonly IEnumerable<Privileges> _privileges = [Models.Privileges.GetWeather];

    #endregion Properties

    #region CTOR

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="privileges"></param>
    public BaseCustomAuthorizationAttribute(params Privileges[] privileges)
    {
        if (privileges.Length > 0)
        {
            Privileges = [.. privileges.Distinct()];
        }
    }

    #endregion CTOR

    #region Authorize

    /// <summary>
    /// 
    /// </summary>
    /// <param name="authorizationFilterContext"></param>
    /// <returns></returns>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext authorizationFilterContext)
    {
        try
        {
            #region Validations

            IEnumerable<Claim> claims = await AuthenticateSchemeAsync(authorizationFilterContext);

            //Validates the app/user has claims
            if (!claims.Any())
            {
                Log.Warning("Authorization : User does not have claims");
                authorizationFilterContext.Result = new ContentResult() { StatusCode = StatusCodes.Status401Unauthorized };
                return;
            }

            string? sub = claims.FirstOrDefault(fd => fd.Type.Equals(JwtClaimTypes.Subject, StringComparison.OrdinalIgnoreCase))?.Value;

            string tokenType = sub != null ? GrantTypes.AuthorizationCode : GrantTypes.ClientCredentials;

            //check if the token is contextual
            if (!IsValidGrantType(sub, tokenType))
            {
                Log.Warning("Authorization : Token is not from a valid grant type");
                authorizationFilterContext.Result = new ContentResult() { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }

            // Validate privilege
            if (!ValidatePrivileges(_privileges, Privileges))
            {
                Log.Warning("Authorization : Invalid privileges");
                authorizationFilterContext.Result = new ContentResult() { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }

            #endregion Validations

            authorizationFilterContext.HttpContext.Items.Add("PrivilegesContext", _privileges ?? []);
        }
        catch (Exception e)
        {
            Log.Error(e, "Authorization : Error validating access token");
            authorizationFilterContext.Result = new ContentResult() { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }
    }

    #endregion Authorize

    #region Virtuals

    /// <summary>
    /// Check if the token is the proper grant type
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="tokenType"></param>
    /// <returns></returns>
    protected virtual bool IsValidGrantType(string? subject, string tokenType)
    {
        return true;
    }

    /// <summary>
    /// Validates the privileges
    /// </summary>
    /// <param name="privileges"></param>
    /// <param name="endpointPrivileges"></param>
    /// <returns></returns>
    protected virtual bool ValidatePrivileges(IEnumerable<Privileges>? privileges, IList<Privileges> endpointPrivileges)
    {
        if (endpointPrivileges.Count == 0)
        {
            return true;
        }

        privileges ??= [];
        if (endpointPrivileges.Any(x => privileges.Any(y => y == x)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Return user claims.
    /// </summary>
    /// <param name="authorizationFilterContext">Authorization filter authorizationFilterContext</param>
    /// <returns>List of Claims</returns>
    protected virtual async Task<IEnumerable<Claim>> AuthenticateSchemeAsync(AuthorizationFilterContext authorizationFilterContext)
    {
        //Validates the jwt token is valid
        if (authorizationFilterContext.HttpContext.User.Identity == null || !authorizationFilterContext.HttpContext.User.Identity.IsAuthenticated)
        {
            Log.Warning("Authorization : User is not authenticated");
            return await Task.FromResult(Enumerable.Empty<Claim>());
        }

        return authorizationFilterContext.HttpContext.User.Claims;
    }

    #endregion Virtuals
}