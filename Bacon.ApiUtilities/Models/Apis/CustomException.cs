using System.Globalization;
using System.Net;

namespace Bacon.ApiUtilities.Models.Apis;

/// <summary>
/// Handles the custom exceptions in web api
/// </summary>
/// <remarks>
/// Constructor with the optional replace parameter
/// </remarks>
/// <param name="errorCode">HttpStatusCode</param>
/// <param name="internalCode">Internal error code</param>
/// <param name="errorMessageData">Error message parameters</param>
public sealed class CustomException(HttpStatusCode errorCode, long internalCode, params object[]? errorMessageData) : Exception(internalCode.ToString(CultureInfo.InvariantCulture))
{
    /// <summary>
    /// The HTTP Error code
    /// </summary>
    public HttpStatusCode ErrorCode { get; } = errorCode;

    /// <summary>
    /// The internal error code
    /// </summary>
    public long InternalCode { get; } = internalCode;

    /// <summary>
    /// Optional arguments. Is used for string replace of the messages
    /// </summary>
    public IEnumerable<object>? ErrorMessageData { get; } = errorMessageData?.Length > 0 ? errorMessageData : null;
}