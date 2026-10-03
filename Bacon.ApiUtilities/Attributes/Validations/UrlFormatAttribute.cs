using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Url format validation attribute
/// </summary>
/// <param name="uriKind">The uri kind to validate against</param>
/// <param name="maxUriLength">The max length the uri can have. Defaults to 2000</param>
public sealed class UrlFormatAttribute(UriKind uriKind, ushort maxUriLength = 2000) : ValidationAttribute
{
    /// <summary>
    /// Check if the validation of the value passed or not
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <param name="validationContext">Validation context</param>
    /// <returns>ValidationResult</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan<ushort>(maxUriLength, 4000);

        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is not string)
        {
            throw new InvalidCastException("The value is not a string");
        }

        string url = value.ToString()!;

        if (url.Length > maxUriLength)
        {
            throw new ArgumentException($"The url cannot have more {maxUriLength}  characters");
        }

        bool success = uriKind == UriKind.RelativeOrAbsolute ? Uri.TryCreate(url, url.StartsWith('/') ? UriKind.Relative : UriKind.Absolute, out _) : Uri.TryCreate(url, uriKind, out _);

        if (url.Length <= maxUriLength && success)
        {
            return ValidationResult.Success;
        }

        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();
        return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.UrlFormatAttribute);
    }
}