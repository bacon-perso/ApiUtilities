using Asp.Versioning;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Samples.Resources;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Frozen;
using System.Text.Json;

namespace Bacon.ApiUtilities.Samples.ProjectConfigs;

/// <summary>
/// 
/// </summary>
public class ApiVersioningErrorResponseProvider(IOptions<JsonOptions> options) : ErrorObjectWriter(options)
{
    #region Properties

    private readonly FrozenDictionary<int, long> _codes = new Dictionary<int, long> { { 400, 400000 }, { 404, 404000 }, { 405, 405000 }, { 415, 415000 } }.ToFrozenDictionary();

    #endregion Properties

    /// <summary>
    /// 
    /// </summary>
    /// <param name="problemDetailsContext"></param>
    /// <returns></returns>
    public override bool CanWrite(ProblemDetailsContext problemDetailsContext)
    {
        ArgumentNullException.ThrowIfNull(problemDetailsContext);

        return problemDetailsContext.ProblemDetails is { Status: { } status } && _codes.ContainsKey(status);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="problemDetailsContext"></param>
    /// <returns></returns>
    public override async ValueTask WriteAsync(ProblemDetailsContext problemDetailsContext)
    {
        ArgumentNullException.ThrowIfNull(problemDetailsContext);

        IStringLocalizer<ErrorCodeResources> errorLocalizer = problemDetailsContext.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<ErrorCodeResources>>();

        string requestUrl = $"{problemDetailsContext.HttpContext.Request.Scheme}://{problemDetailsContext.HttpContext.Request.Host}{problemDetailsContext.HttpContext.Request.Path}";

        int httpStatusCode = problemDetailsContext.ProblemDetails.Status!.Value;
        long internalErrorCode = _codes[httpStatusCode];

        string resourceKey = internalErrorCode.ToString();

        bool is404 = httpStatusCode == StatusCodes.Status404NotFound;

        ErrorResult errorResult = new()
        {
            InternalCode = internalErrorCode,
            HttpStatusCode = httpStatusCode,
            Message = is404 ? errorLocalizer[resourceKey, requestUrl] : errorLocalizer[resourceKey],
            Metadata = is404 ? [requestUrl] : null,
        };

        problemDetailsContext.HttpContext.Response.StatusCode = httpStatusCode;

        await problemDetailsContext.HttpContext.Response.WriteAsJsonAsync(errorResult, JsonSerializerOptions.Web);
    }
}