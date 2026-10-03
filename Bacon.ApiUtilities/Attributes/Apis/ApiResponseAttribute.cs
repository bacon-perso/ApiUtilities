using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Bacon.ApiUtilities.Attributes.Apis;

/// <summary>
/// Api response attribute
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiResponseAttribute(Type type, int statusCode, string contentType, params string[] additionalContentTypes) : ProducesResponseTypeAttribute(type, statusCode, contentType, additionalContentTypes)
{
    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="statusCode">The Http status code</param>
    public ApiResponseAttribute(int statusCode) : this(typeof(void), statusCode, MediaTypeNames.Application.Json)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">The type of object returned</param>
    /// <param name="statusCode">The Http status code</param>
    public ApiResponseAttribute(Type type, int statusCode) : this(type, statusCode, MediaTypeNames.Application.Json)
    {
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="type">The type of object returned</param>
    /// <param name="additionalContentTypes">Additional media content types. IE: application/json</param>
    public ApiResponseAttribute(Type type, params string[] additionalContentTypes) : this(type, StatusCodes.Status200OK, MediaTypeNames.Application.Json, additionalContentTypes)
    {
    }
}