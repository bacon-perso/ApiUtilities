using Swashbuckle.AspNetCore.SwaggerUI;

namespace Bacon.ApiUtilities.Models.Apis.Documentations;

/// <summary>
/// Defines the configs for the Open API UI
/// </summary>
public sealed class OpenApiUiConfigs
{
    /// <summary>
    /// Defines the UI the use with to document the open API
    /// </summary>
    public UiTypes UiType { get; set; } = UiTypes.Swagger;

    /// <summary>
    /// Defines the path used for the UI. Defaults to empty string
    /// </summary>
    public string VirtualPath { get; set; } = string.Empty;

    /// <summary>
    /// Cors policy privacy color css. The key is the corsPolicyName, the value is the hex color
    /// </summary>
    public Dictionary<string, string>? CorsPolicyCss { get; set; } = [];

    /// <summary>
    /// List of HTTP methods that have the Try it out feature enabled. An empty array disables Try it out for all operations.
    /// This does not filter the operations from the display
    /// </summary>
    public SubmitMethod[]? SupportedSubmitMethods { get; set; }

    /// <summary>
    /// Controls the default expansion setting for the endpoints. If true, all endpoints are collapsed
    /// </summary>
    public bool CollapseEndpoints { get; set; } = true;

    internal string NormalizedVirtualPath => (VirtualPath ?? string.Empty).Trim('/');
}