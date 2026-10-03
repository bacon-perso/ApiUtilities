using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bacon.ApiUtilities.Services.ModelStateValidations;

internal sealed class ModelValidationService(IErrorLocalizerService customErrorLocalizerService, IOptions<ModelStateValidationOptions> modelStateValidationOptions) : IModelValidationService
{
    public ValidationResult CreateInvalidModelValidationResult(ValidationContext validationContext, ValidationAttributeTypes validationAttributeType, params object[] errorMessageData)
    {
        long internalErrorCode = (long)HttpStatusCode.PreconditionFailed;
        string resourceKey = string.Empty;

        ModelStateValidationOptions options = modelStateValidationOptions.Value;

        if (options.BuiltInValidationAttributeSettings != null && options.BuiltInValidationAttributeSettings.TryGetValue(validationAttributeType, out long errorCode))
        {
            internalErrorCode = errorCode;
            resourceKey = errorCode.ToString(CultureInfo.InvariantCulture);
        }

        HttpStatusCode httpStatusCode = InternalErrorCodesHelper.TryGetHttpStatusCode(internalErrorCode, out HttpStatusCode? statusCode) ? statusCode.Value : HttpStatusCode.PreconditionFailed;

        return CreateInvalidModelValidationResult(validationContext, resourceKey, httpStatusCode, internalErrorCode, errorMessageData);
    }

    public ValidationResult CreateInvalidModelValidationResult(ValidationContext validationContext, string errorResourceKey, HttpStatusCode httpStatusCode, long internalErrorCode, params object[] errorMessageData)
    {
        string parameterName = validationContext.DisplayName;

        // if property == null, we consider it as parameter name, otherwise a property name
        string fieldName = validationContext.ObjectType.GetProperty(parameterName)?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? parameterName;

        object[] errorMessageParams = [fieldName, .. errorMessageData];

        ErrorResult cm = new()
        {
            InternalCode = internalErrorCode,
            HttpStatusCode = (int)httpStatusCode,
            Message = MessageFormatterService.FormatErrorResultMessage(customErrorLocalizerService.GetResourceValue(errorResourceKey), errorMessageParams),
            Metadata = errorMessageParams
        };

        return new ValidationResult(JsonSerializer.Serialize(cm, JsonSerializerOptions.Web));
    }
}