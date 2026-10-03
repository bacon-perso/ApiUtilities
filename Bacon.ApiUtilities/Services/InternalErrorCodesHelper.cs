using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Bacon.ApiUtilities.Services;

internal static class InternalErrorCodesHelper
{
    public static bool TryGetHttpStatusCode(long internalErrorCode, [NotNullWhen(true)] out HttpStatusCode? httpStatusCode)
    {
        httpStatusCode = null;

        if (internalErrorCode < 100)
        {
            return false;
        }

        long code = internalErrorCode;
        while (code >= 1000)
        {
            code /= 10;
        }

        HttpStatusCode potentialHttpStatusCode = (HttpStatusCode)(int)code;

        if (!Enum.IsDefined(potentialHttpStatusCode))
        {
            return false;
        }

        httpStatusCode = potentialHttpStatusCode;
        return true;
    }
}