using Bacon.ApiUtilities.Attributes.Validations;
using System.Text.Json.Serialization;

namespace Bacon.ApiUtilities.Samples.DTOs;

/// <summary>
/// Caption DTO
/// </summary>
public class CaptionSet
{
    /// <summary>
    /// Defines the language code, IE : en-US.
    /// </summary>
    [RequiredField]
    public string? LanguageCode { get; set; }

    /// <summary>
    /// The text
    /// </summary>
    [RequiredField]
    public string? Text { get; set; }
}