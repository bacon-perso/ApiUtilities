using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace Bacon.ApiUtilities.Services.ModelStateValidations;

internal sealed partial class ModelStateApiBehaviorOptionsService(IErrorLocalizerService errorLocalizerService, IOptions<ModelStateValidationOptions> modelStateValidationOptions, ILogger<ModelStateApiBehaviorOptionsService> logger) : IConfigureOptions<ApiBehaviorOptions>
{
    public void Configure(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = HandleInvalidModelStateResponse;
    }

    #region Private

    private ObjectResult HandleInvalidModelStateResponse(ActionContext actionContext)
    {
        long unhandledInternalErrorCode = modelStateValidationOptions.Value.UnhandledValidationInternalErrorCode;

        List<ErrorResult> errors = [];
        foreach (KeyValuePair<string, ModelStateEntry> modelStateEntries in actionContext.ModelState)
        {
            if (modelStateEntries.Value is not { ValidationState: ModelValidationState.Invalid } modelStateEntry)
            {
                continue;
            }

            errors.Add(ToErrorResult(modelStateEntries.Key, modelStateEntry, unhandledInternalErrorCode));
        }

        if (errors.Count == 0)
        {
            errors.Add(CreateUnhandledErrorResult(unhandledInternalErrorCode, string.Empty));
        }

        ErrorResult errorResult = errors.FirstOrDefault(fd => fd.InternalCode == unhandledInternalErrorCode) ?? errors[0];

        LogModelStateValidationObjectResult(errorResult.HttpStatusCode, errorResult.InternalCode, errorResult.Message);

        return new ObjectResult(errorResult) { StatusCode = errorResult.HttpStatusCode };
    }

    private ErrorResult ToErrorResult(string key, ModelStateEntry? modelStateEntry, long unhandledInternalErrorCode)
    {
        ModelError? modelError = modelStateEntry?.Errors.Count > 0 ? modelStateEntry.Errors[0] : null;

        if (modelError is null)
        {
            return CreateUnhandledErrorResult(unhandledInternalErrorCode, key);
        }

        if (modelError.Exception != null && string.IsNullOrWhiteSpace(modelError.ErrorMessage))
        {
            return CreateUnhandledErrorResult(unhandledInternalErrorCode, modelError.Exception.Message);
        }

        try
        {
            ErrorResult? errorResult = JsonSerializer.Deserialize<ErrorResult>(modelError.ErrorMessage, JsonSerializerOptions.Web);

            if (errorResult is not null)
            {
                return errorResult;
            }
        }
        catch (JsonException)
        {
            //deserialization failed
        }

        return CreateUnhandledErrorResult(unhandledInternalErrorCode, key);
    }

    private ErrorResult CreateUnhandledErrorResult(long unhandledInternalErrorCode, string message)
    {
        CustomException customException = new(HttpStatusCode.PreconditionFailed, unhandledInternalErrorCode, message);

        return new(customException, errorLocalizerService.GetResourceValue(unhandledInternalErrorCode.ToString(CultureInfo.InvariantCulture)));
    }

    #region Logs

    [LoggerMessage(Level = LogLevel.Warning, Message = "Validation error = {{ Error code: {HttpStatusCode}, Internal error code: {InternalCode}, Message: {ErrorMessage} }}")]
    private partial void LogModelStateValidationObjectResult(int httpStatusCode, long internalCode, string errorMessage);

    #endregion Logs

    #endregion Private
}