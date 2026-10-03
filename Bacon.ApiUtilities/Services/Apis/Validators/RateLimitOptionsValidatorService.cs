using Bacon.ApiUtilities.Models.Apis.RateLimits;
using Microsoft.Extensions.Options;
using System.Net;

namespace Bacon.ApiUtilities.Services.Apis.Validators;

internal sealed class RateLimitOptionsValidatorService : IValidateOptions<RateLimitOptions>
{
    public ValidateOptionsResult Validate(string? name, RateLimitOptions rateLimitOptions)
    {
        if (rateLimitOptions.RateLimitInternalErrorCode <= 0)
        {
            return ValidateOptionsResult.Fail("Invalid rate limit error response code. The code cannot be 0 or less than 0");
        }

        if (!InternalErrorCodesHelper.TryGetHttpStatusCode(rateLimitOptions.RateLimitInternalErrorCode, out HttpStatusCode? rateLimitHttpStatusCode))
        {
            return ValidateOptionsResult.Fail("Invalid rate limiting error response code format");
        }

        if (!HttpStatusCode.TooManyRequests.Equals(rateLimitHttpStatusCode.Value))
        {
            return ValidateOptionsResult.Fail("Invalid rate limit response code. The first 3 digit must be 429");
        }

        return ValidateOptionsResult.Success;
    }
}