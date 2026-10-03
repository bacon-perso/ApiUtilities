using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Samples.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Bacon.ApiUtilities.Samples.Attributes.Validations;

/// <summary>
/// 
/// </summary>
public class AnotherValidationAttribute : ValidationAttribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is WeatherForecast)
        {
            return ValidationResult.Success;
        }

        IModelValidationService modelStateValidation = validationContext.GetRequiredService<IModelValidationService>();

        return modelStateValidation.CreateInvalidModelValidationResult(validationContext, "412013", HttpStatusCode.PreconditionFailed, 412013);
    }
}