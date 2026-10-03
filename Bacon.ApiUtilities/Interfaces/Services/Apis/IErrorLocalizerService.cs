namespace Bacon.ApiUtilities.Interfaces.Services.Apis;

/// <summary>
/// Defines the helper service that will read through the resources
/// </summary>
public interface IErrorLocalizerService
{
    /// <summary>
    /// Get the resource value linked to the key sent
    /// </summary>
    /// <param name="resourceKey">The resource key</param>
    /// <returns>The resource value. Null if not found</returns>
    string GetResourceValue(string resourceKey);
}