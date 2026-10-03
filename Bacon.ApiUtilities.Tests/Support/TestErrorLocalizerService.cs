using Bacon.ApiUtilities.Interfaces.Services.Apis;
using System.Globalization;

namespace Bacon.ApiUtilities.Tests.Support;

/// <summary>
/// Marker type required by AddApiExceptionHandler. The resources are never read because <see cref="TestErrorLocalizerService"/> replaces the localizer.
/// </summary>
internal sealed class TestResources
{
}

/// <summary>
/// Returns a fixed message template per internal error code, so tests do not depend on .resx files
/// </summary>
internal sealed class TestErrorLocalizerService : IErrorLocalizerService
{
    public const string RateLimitMessageTemplate = "You may only perform this action every {0} milliseconds.";

    public string GetResourceValue(string resourceKey)
    {
        return resourceKey == RateLimitTestHost.RateLimitInternalErrorCode.ToString(CultureInfo.InvariantCulture) ? RateLimitMessageTemplate : resourceKey;
    }
}
