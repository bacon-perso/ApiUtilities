using Bacon.ApiUtilities.Models.Apis;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Bacon.ApiUtilities.Extensions;

/// <summary>
/// Boolean extensions
/// </summary>
public static class BooleanExtensions
{
    #region Publics

    extension(bool value)
    {
        /// <summary>
        /// Throw custom exception if invalid
        /// </summary>
        /// <param name="httpStatusCode">external http status code</param>
        /// <param name="internalExceptionCode">internal exception code</param>
        /// <param name="errorMessageData">The extra parameters passed to the custom exception</param>
        public void ThrowCustomExceptionIfTrue(HttpStatusCode httpStatusCode, long internalExceptionCode, params object[]? errorMessageData)
        {
            if (value)
            {
                ThrowCustomException(httpStatusCode, internalExceptionCode, errorMessageData);
            }
        }

        /// <summary>
        /// Throw custom exception if invalid
        /// </summary>
        /// <param name="httpStatusCode">external http status code</param>
        /// <param name="internalExceptionCode">internal exception code</param>
        /// <param name="errorMessageData">The extra parameters passed to the custom exception</param>
        public void ThrowCustomExceptionIfFalse(HttpStatusCode httpStatusCode, long internalExceptionCode, params object[]? errorMessageData)
        {
            if (!value)
            {
                ThrowCustomException(httpStatusCode, internalExceptionCode, errorMessageData);
            }
        }
    }

    #endregion Publics

    #region Privates

    [DoesNotReturn]
    private static void ThrowCustomException(HttpStatusCode httpStatusCode, long internalExceptionCode, params object[]? errorMessageData)
    {
        throw new CustomException(httpStatusCode, internalExceptionCode, errorMessageData);
    }

    #endregion Privates
}