using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Custom Range attribute
/// Checks if a value is in between minimum and maximum value.
/// </summary>
/// <param name="minimum">Minimum value</param>
/// <param name="maximum">Maximum value</param>
public sealed class AllowedRangeAttribute(double minimum, double maximum) : RangeAttribute(minimum, maximum)
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

        return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.AllowedRangeAttribute, Minimum, Maximum);
    }
}