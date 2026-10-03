using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Bacon.ApiUtilities.Tests.Support;

/// <summary>
/// Rate limiting configured by the application itself, independently of the library.
/// Used to check that AddApiRateLimiting does not overwrite it.
/// </summary>
internal static class ConsumerRateLimiting
{
    public const string PolicyName = "consumer";

    public const string GlobalLimitedPath = "/ratelimit/consumer-global";

    public const string HandlerHeaderName = "X-Consumer-Handler";

    /// <summary>
    /// Must be called before AddApiRateLimiting
    /// </summary>
    public static void Add(IServiceCollection services)
    {
        services.AddRateLimiter(o =>
        {
            o.AddFixedWindowLimiter(PolicyName, l =>
            {
                l.PermitLimit = 1;
                l.Window = TimeSpan.FromMinutes(1);
                l.QueueLimit = 0;
            });

            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (httpContext.Request.Path != GlobalLimitedPath)
                {
                    return RateLimitPartition.GetNoLimiter("consumer-none");
                }

                return RateLimitPartition.GetFixedWindowLimiter("consumer-global", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });

            o.OnRejected = (onRejectedContext, cancellationToken) =>
            {
                onRejectedContext.HttpContext.Response.Headers[HandlerHeaderName] = "true";

                return ValueTask.CompletedTask;
            };
        });
    }
}
