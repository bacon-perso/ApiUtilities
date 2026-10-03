namespace Bacon.ApiUtilities.Interfaces.Services.Apis;

/// <summary>
/// Endpoint access privileges interface
/// </summary>
/// <typeparam name="TPrivilege">Type containing privilege information</typeparam>
public interface IEndpointAccessPrivileges<TPrivilege>
{
    /// <summary>
    /// Privilege list
    /// </summary>
    IList<TPrivilege> Privileges { get; }
}