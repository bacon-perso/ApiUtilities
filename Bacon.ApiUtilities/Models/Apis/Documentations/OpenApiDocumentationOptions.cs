using Bacon.ApiUtilities.Attributes.Apis;

namespace Bacon.ApiUtilities.Models.Apis.Documentations;

/// <summary>
/// Defines the open API configurations
/// </summary>
public sealed class OpenApiDocumentationOptions
{
    /// <summary>
    /// Defines the documents used in the open API generation
    /// </summary>
    public required IEnumerable<OpenApiDocumentInfo> OpenApiDocumentInfos { get; set; }

    /// <summary>
    /// Defines if endpoints using <see cref="HiddenApiAttribute"/> should be hidden or not
    /// </summary>
    public bool DisplayHiddenEndpoints { get; set; } = true;
    
    /// <summary>
    /// The duration of the cache when generation the open API document. Defaults to no cache
    /// </summary>
    public TimeSpan OutputCacheDuration { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Defines the configs for the Open API UI
    /// </summary>
    public OpenApiUiConfigs UiConfigs { get; set; } = new();
}