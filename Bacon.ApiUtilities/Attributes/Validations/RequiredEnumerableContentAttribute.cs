using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Validates that the collection is either null or each of its values are not null
/// </summary>
public sealed class RequiredEnumerableContentAttribute : ValidationAttribute
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

        if (value is not IEnumerable items)
        {
            throw new InvalidCastException($"{nameof(RequiredEnumerableContentAttribute)} can only be applied to enumerables");
        }

        foreach (object? item in items)
        {
            if (item == null)
            {
                IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

                return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.RequiredEnumerableContentAttribute);
            }
        }

        return ValidationResult.Success;
    }
}