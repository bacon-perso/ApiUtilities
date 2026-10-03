using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Bacon.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Validate if the value respects email format
/// </summary>
public sealed class EmailFormatAttribute : ValidationAttribute
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

        if (value is not string str)
        {
            throw new InvalidCastException($"{nameof(EmailFormatAttribute)} can only be applied on string");
        }

        try
        {
            if (Validators.ValidateEmail(str))
            {
                return ValidationResult.Success;
            }
        }
        catch
        {
        }

        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

        return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.EmailFormatAttribute);
    }
}