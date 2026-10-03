using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Checks if the required property is not null or empty.
/// </summary>
public sealed class RequiredFieldAttribute : RequiredAttribute
{
    /// <summary>
    /// Check if the validation of the value passed or not
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <param name="validationContext">Validation context</param>
    /// <returns>ValidationResult</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (!base.IsValid(value))
        {
            return CreateInvalidResult(validationContext);
        }

        if (value is string || value is not IEnumerable enumerable)
        {
            return ValidationResult.Success;
        }

        bool hasItems = false;
        foreach (object? item in enumerable)
        {
            hasItems = true;

            if (!base.IsValid(item))
            {
                return CreateInvalidResult(validationContext);
            }
        }

        return hasItems ? ValidationResult.Success : CreateInvalidResult(validationContext);
    }

    private static ValidationResult CreateInvalidResult(ValidationContext validationContext)
    {
        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

        return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.RequiredFieldAttribute);
    }
}