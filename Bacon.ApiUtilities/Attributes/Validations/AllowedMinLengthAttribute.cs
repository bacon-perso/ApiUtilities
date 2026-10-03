using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Custom minlegth attribute
/// Checks if a string or list respects the minimum length (count).
/// </summary>
/// <param name="length">Allowed min length</param>
public sealed class AllowedMinLengthAttribute(int length) : MinLengthAttribute(length)
{
    /// <summary>
    /// Check if the validation of the value passed or not
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <param name="validationContext">Validation context</param>
    /// <returns>ValidationResult</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (base.IsValid(value))
        {
            return ValidationResult.Success;
        }

        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

        return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.AllowedMinLengthAttribute, Length);
    }
}