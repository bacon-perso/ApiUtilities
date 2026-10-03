using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Validation attribute to check if the list contains duplicated items
/// </summary>
public sealed class DuplicatedItemsAttribute : ValidationAttribute
{
    /// <summary>
    /// Check if the validation of the value passed or not
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <param name="validationContext">Validation context</param>
    /// <returns>ValidationResult</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        //For IDictionary, doesn't make sense to use this attribute
        //because it is always unique
        if (value is not IEnumerable list || value is IDictionary)
        {
            throw new InvalidCastException("The value is not an instance of IEnumerable or IDictionary");
        }

        HashSet<object?> hashset = [];

        foreach (object? o in list)
        {
            if (!hashset.Add(o))
            {
                IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();
                return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.DuplicatedItemsAttribute);
            }
        }

        return ValidationResult.Success;
    }
}