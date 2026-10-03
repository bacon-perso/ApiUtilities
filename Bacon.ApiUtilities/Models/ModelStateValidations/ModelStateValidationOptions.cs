namespace Bacon.ApiUtilities.Models.ModelStateValidations;

/// <summary>
/// Model state validation options
/// </summary>
public sealed class ModelStateValidationOptions
{
    /// <summary>
    /// Built-in validation attribute setting dictionary
    /// </summary>
    public IDictionary<ValidationAttributeTypes, long>? BuiltInValidationAttributeSettings { get; set; }

    /// <summary>
    /// Custom validation attribute setting dictionary
    /// </summary>
    public IDictionary<Type, IEnumerable<long>>? CustomValidationAttributeSettings { get; set; }

    /// <summary>
    /// Internal error code for unhandled validation error
    /// </summary>
    public long UnhandledValidationInternalErrorCode { get; set; }

    /// <summary>
    /// Internal error code for Internal server error
    /// </summary>
    public long InternalServerErrorInternalErrorCode { get; set; }
}