using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Utc date time format validation attribute
/// </summary>
public sealed class UtcDateTimeFormatAttribute : ValidationAttribute
{
    /// <summary>
    /// Check if the validation of the value passed or not
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <param name="validationContext">Validation context</param>
    /// <returns>ValidationResult</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is not DateTime dateTime)
        {
            throw new InvalidCastException($"{nameof(UtcDateTimeFormatAttribute)} can only be applied to DateTime properties");
        }

        if (dateTime.Kind == DateTimeKind.Utc)
        {
            return ValidationResult.Success;
        }

        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

        return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.UtcDateTimeFormatAttribute);
    }
}