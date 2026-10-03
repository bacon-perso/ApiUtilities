using Bacon.ApiUtilities.Models.Apis;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using System.Text.Json;

namespace Bacon.ApiUtilities.Services;

internal static class ErrorResponseWriterService
{
    public static ValueTask WriteAsync(HttpContext httpContext, ErrorResult errorResult)
    {
        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(errorResult, JsonSerializerOptions.Web);

        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = errorResult.HttpStatusCode;
        httpContext.Response.ContentLength = jsonBytes.Length;

        return httpContext.Response.Body.WriteAsync(jsonBytes);
    }
}