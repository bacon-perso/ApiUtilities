using Bacon.ApiUtilities.Models.ModelStateValidations;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Bacon.ApiUtilities.Services.Apis.Validators;

internal sealed class ModelStateValidationOptionsValidatorService : IValidateOptions<ModelStateValidationOptions>
{
    public ValidateOptionsResult Validate(string? name, ModelStateValidationOptions modelStateValidationOptions)
    {
        List<string> errors = [];

        ValidateUnhandledValidation(modelStateValidationOptions, errors);
        ValidateInternalServerErrorValidation(modelStateValidationOptions, errors);

        List<(string TypeName, long InternalErrorCode)> validationAttributes = [];

        #region Add built-in validation attributes to validations attributes

        if (modelStateValidationOptions.BuiltInValidationAttributeSettings is { Count: > 0 } builtInAttributes)
        {
            validationAttributes.AddRange(builtInAttributes.Select(s => (s.Key.ToString(), s.Value)));
        }

        #endregion Add built-in validation attributes to validations attributes

        #region Validate custom validation attributes and add to validation attributes

        if (modelStateValidationOptions.CustomValidationAttributeSettings is { Count: > 0 } customAttributes)
        {
            string[] invalidCustomValidationAttributes = [.. customAttributes.Where(w => !typeof(ValidationAttribute).IsAssignableFrom(w.Key)).Select(s => s.Key.Name)];
            if (invalidCustomValidationAttributes.Length != 0)
            {
                errors.Add($"The following custom validation attributes are invalid (not assignable from ValidationAttribute): {string.Join(",", invalidCustomValidationAttributes)}");
            }
            else
            {
                foreach (KeyValuePair<Type, IEnumerable<long>> kvp in customAttributes)
                {
                    foreach (long internalErrorCode in kvp.Value)
                    {
                        validationAttributes.Add((kvp.Key.FullName ?? kvp.Key.Name, internalErrorCode));
                    }
                }
            }
        }

        #endregion Validate custom validation attributes and add to validation attributes

        ValidateValidationAttributes(validationAttributes, errors);

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    #region Validate unhandled validation

    private static void ValidateUnhandledValidation(ModelStateValidationOptions modelStateValidationOptions, List<string> errors)
    {
        if (modelStateValidationOptions.UnhandledValidationInternalErrorCode <= 0)
        {
            errors.Add("Invalid empty unhandled validation internal error code. The code cannot be 0 or less than 0");
            return;
        }

        if (!InternalErrorCodesHelper.TryGetHttpStatusCode(modelStateValidationOptions.UnhandledValidationInternalErrorCode, out _))
        {
            errors.Add("Invalid unhandled validation internal error code format");
        }
    }

    #endregion Validate unhandled validation

    #region Validate internal server error validation

    private static void ValidateInternalServerErrorValidation(ModelStateValidationOptions modelStateValidationOptions, List<string> errors)
    {
        if (modelStateValidationOptions.InternalServerErrorInternalErrorCode <= 0)
        {
            errors.Add("Invalid internal server error response code. The code cannot be 0 or less than 0");
            return;
        }

        if (!InternalErrorCodesHelper.TryGetHttpStatusCode(modelStateValidationOptions.InternalServerErrorInternalErrorCode, out HttpStatusCode? internalServerErrorHttpStatusCode))
        {
            errors.Add("Invalid internal server error response code format");
            return;
        }

        if (!HttpStatusCode.InternalServerError.Equals(internalServerErrorHttpStatusCode.Value))
        {
            errors.Add("Invalid internal server error response code. The code is not 500");
        }
    }

    #endregion Validate internal server error validation

    #region Validate validation attributes

    private static void ValidateValidationAttributes(IEnumerable<(string TypeName, long InternalErrorCode)> validationAttributes, List<string> errors)
    {
        #region Validate internal error code is not less or equal to 0

        string[] validationAttributesWithInvalidInternalErrorCode = [.. validationAttributes.Where(w => w.InternalErrorCode <= 0).Select(s => s.TypeName)];
        if (validationAttributesWithInvalidInternalErrorCode.Length != 0)
        {
            errors.Add($"The following validation attribute types are invalid. The internal error code cannot be 0 or less than 0: {string.Join(", ", validationAttributesWithInvalidInternalErrorCode)}");
            return;
        }

        #endregion Validate internal error code is not less or equal to 0

        #region Validate internal error code has correct format

        List<string> invalidInternalErrorCodeFormat = [];
        foreach ((string typeName, long internalErrorCode) in validationAttributes)
        {
            if (!InternalErrorCodesHelper.TryGetHttpStatusCode(internalErrorCode, out _))
            {
                invalidInternalErrorCodeFormat.Add(typeName);
            }
        }

        if (invalidInternalErrorCodeFormat.Count != 0)
        {
            errors.Add($"The following validation attribute types have invalid internal error code format: {string.Join(", ", invalidInternalErrorCodeFormat)}");
        }

        #endregion Validate internal error code has correct format
    }

    #endregion Validate validation attributes
}