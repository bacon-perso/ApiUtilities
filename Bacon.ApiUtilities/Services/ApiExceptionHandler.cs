using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;

namespace Bacon.ApiUtilities.Services;

internal sealed partial class ApiExceptionHandler(IErrorLocalizerService errorLocalizerService, IOptions<ModelStateValidationOptions> modelStateValidationOptions, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Client aborts (499) and responses that already started are handled by the framework's ExceptionHandlerMiddleware before reaching this handler
        ErrorResult errorResult = exception is CustomException customException ? CreateCustomExceptionResult(httpContext, customException) : CreateInternalServerErrorResult(httpContext, exception);

        await ErrorResponseWriterService.WriteAsync(httpContext, errorResult);

        // true = handled. This also stops the framework from logging the exception a second time
        return true;
    }

    #region Privates

    private ErrorResult CreateCustomExceptionResult(HttpContext httpContext, CustomException customException)
    {
        LogCustomException(httpContext.Request.Method, httpContext.Request.Path, customException.InternalCode, customException);

        return new(customException, errorLocalizerService.GetResourceValue(customException.InternalCode.ToString(CultureInfo.InvariantCulture)));
    }

    private ErrorResult CreateInternalServerErrorResult(HttpContext httpContext, Exception exception)
    {
        long internalServerErrorCode = modelStateValidationOptions.Value.InternalServerErrorInternalErrorCode;

        LogInternalServerError(httpContext.Request.Method, httpContext.Request.Path, exception);

        return new()
        {
            HttpStatusCode = (int)HttpStatusCode.InternalServerError,
            InternalCode = internalServerErrorCode,
            Message = errorLocalizerService.GetResourceValue(internalServerErrorCode.ToString(CultureInfo.InvariantCulture))
        };
    }

    #region Logs

    [LoggerMessage(Level = LogLevel.Warning, Message = "Error occurred while calling endpoint {Method}: {Path}. Error Code : {InternalCode}")]
    private partial void LogCustomException(string method, string path, long internalCode, CustomException customException);

    [LoggerMessage(Level = LogLevel.Error, Message = "Internal error occurred while calling endpoint {Method}: {Path}")]
    private partial void LogInternalServerError(string method, string path, Exception exception);

    #endregion Logs

    #endregion Privates
}