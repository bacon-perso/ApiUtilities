using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using PhoneNumbers;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Validate if the value respects a phone number format
/// </summary>
public sealed class PhoneNumberFormatAttribute : ValidationAttribute
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

        try
        {
            PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber phone = phoneNumberUtil.Parse(value.ToString(), null);

            bool isValidNumber = phoneNumberUtil.IsValidNumber(phone);

            if (isValidNumber)
            {
                return ValidationResult.Success;
            }
        }
        catch
        {
        }

        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

        return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.PhoneNumberFormatAttribute);
    }
}