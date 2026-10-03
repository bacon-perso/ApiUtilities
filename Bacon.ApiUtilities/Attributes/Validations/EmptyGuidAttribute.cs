using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Attributes.Validations;

/// <summary>
/// Checks if the Guid value is empty
/// </summary>
public sealed class EmptyGuidAttribute : ValidationAttribute
{
    /// <summary>
    /// Check if the validation of the value passed or not
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <param name="validationContext">Validation context</param>
    /// <returns>ValidationResult</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        //NotEmpty doesn't necessarily mean required
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (!(value is Guid || value is Guid? || value is IEnumerable<Guid> || value is IEnumerable<Guid?>))
        {
            throw new InvalidCastException("The value is not a Guid, Guid?, IEnumerable<Guid> or IEnumerable<Guid?>");
        }

        Type valueType = value.GetType();

        IEnumerable guidList = valueType.GetInterface(nameof(IEnumerable)) != null || valueType.GetInterface(nameof(ICollection)) != null ? (value as IEnumerable)! : new object[] { value };

        foreach (Guid? guid in guidList)
        {
            if (guid == null)
            {
                continue;
            }

            if (guid.Value == Guid.Empty)
            {
                IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();
                return modelStateValidation.CreateInvalidModelValidationResult(validationContext, ValidationAttributeTypes.EmptyGuidAttribute);
            }
        }

        return ValidationResult.Success;
    }
}