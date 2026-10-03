using Bacon.ApiUtilities.Attributes.Validations;

namespace Bacon.ApiUtilities.Samples.DTOs;

/// <summary>
/// Toto Set
/// </summary>
public class TotoSet
{
    /// <summary>
    /// The name
    /// </summary>
    [RequiredField]
    public string? Name { get; set; }

    /// <summary>
    /// The captions
    /// </summary>
    [RequiredField]
    public IEnumerable<CaptionSet>? Captions { get; set; }
}