using Bacon.ApiUtilities.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;

/// <summary>
/// Defines the behavior to trigger the model state validation
/// </summary>
public interface IModelValidationService
{
    /// <summary>
    /// Return the validation result based on validations to trigger to model state validation if needed
    /// </summary>
    /// <param name="validationContext">The validation context</param>
    /// <param name="validationAttributeType">The validation attribute type</param>
    /// <param name="errorMessageData">The extra metadata to add in the ErrorResult</param>
    /// <returns>The ValidationResult</returns>
    ValidationResult CreateInvalidModelValidationResult(ValidationContext validationContext, ValidationAttributeTypes validationAttributeType, params object[] errorMessageData);

    /// <summary>
    /// Return the validation result based on validations to trigger to model state validation if needed
    /// </summary>
    /// <param name="validationContext">The validation context</param>
    /// <param name="errorResourceKey">The resource key. Used to get the error message</param>
    /// <param name="httpStatusCode">The HttpStatusCode (ie: 200)</param>
    /// <param name="internalErrorCode">The internal error code. (ie: 412001 to represent a 412 error)</param>
    /// <param name="errorMessageData">The extra metadata to add in the ErrorResult</param>
    /// <returns>The ValidationResult</returns>
    ValidationResult CreateInvalidModelValidationResult(ValidationContext validationContext, string errorResourceKey, HttpStatusCode httpStatusCode, long internalErrorCode, params object[] errorMessageData);
}