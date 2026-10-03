using Bacon.ApiUtilities.Services;

namespace Bacon.ApiUtilities.Models.Apis;

/// <summary>
/// Error object containing the http status code, the error internal code, the message, and the additional value that got replaced
/// </summary>
public sealed class ErrorResult
{
    #region Properties

    /// <summary>
    /// The HTTP Status Code
    /// </summary>
    public int HttpStatusCode { get; set; }

    /// <summary>
    /// The internal status code
    /// </summary>
    public long InternalCode { get; set; }

    /// <summary>
    /// The message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The list containing the error message data
    /// </summary>
    public IEnumerable<object>? Metadata { get; set; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// CTOR
    /// </summary>
    public ErrorResult()
    {
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="customException">The custom exception</param>
    /// <param name="message">The error message</param>
    public ErrorResult(CustomException customException, string message)
    {
        HttpStatusCode = (int)customException.ErrorCode;
        InternalCode = customException.InternalCode;
        Message = MessageFormatterService.FormatErrorResultMessage(message, customException.ErrorMessageData);
        Metadata = customException.ErrorMessageData;
    }

    #endregion Constructor
}