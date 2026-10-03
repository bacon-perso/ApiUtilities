using Microsoft.OpenApi;

namespace Bacon.ApiUtilities.Models.Apis.Documentations;

/// <summary>
/// Represent an open API document
/// </summary>
public sealed class OpenApiDocumentInfo
{
    /// <summary>
    /// A Unique URI-friendly name that uniquely identifies the document
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The title of the application.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The version of the OpenAPI document.
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// A short description of the application.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Defines the Open API license
    /// </summary>
    public OpenApiLicense? License { get; set; }
}