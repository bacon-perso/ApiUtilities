using Bacon.ApiUtilities.Attributes.Apis;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Models.Apis.RateLimits;
using Bacon.ApiUtilities.Services;
using Bacon.ApiUtilities.Services.Apis.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Bacon.ApiUtilities.Extensions;

/// <summary>
/// Api rate limiter extensions
/// </summary>
public static partial class ApiRateLimiterExtensions
{
    private static readonly ConcurrentDictionary<Endpoint, string> _rateLimitCache = [];

    /// <summary>
    /// Add rate limiting on individual endpoints
    /// </summary>
    /// <param name="apiExceptionHandlerBuilder"></param>
    /// <param name="configureOptions">RateLimitOptions action</param>
    /// <returns></returns>
    public static ApiExceptionHandlerBuilder AddApiRateLimiting(this ApiExceptionHandlerBuilder apiExceptionHandlerBuilder, Action<RateLimitOptions> configureOptions)
    {
        apiExceptionHandlerBuilder.ServiceCollection.AddOptions<RateLimitOptions>()
            .Configure(configureOptions)
            .ValidateOnStart();

        apiExceptionHandlerBuilder.ServiceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<RateLimitOptions>, RateLimitOptionsValidatorService>());

        apiExceptionHandlerBuilder.ServiceCollection.AddRateLimiter(o =>
        {
            // One variable per host: this lambda runs once per host when the limiter options are built
            ILogger? logger = null;

            PartitionedRateLimiter<HttpContext> rateLimitLimiter = PartitionedRateLimiter.Create<HttpContext, string>(CreateRateLimitPartition);

            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.GlobalLimiter = o.GlobalLimiter == null ? rateLimitLimiter : PartitionedRateLimiter.CreateChained(o.GlobalLimiter, rateLimitLimiter);

            Func<OnRejectedContext, CancellationToken, ValueTask>? previousOnRejectedHandle = o.OnRejected;

            o.OnRejected = async (onRejectedContext, cancellationToken) =>
            {
                EndpointRateLimitAttribute? endpointRateLimitAttribute = onRejectedContext.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EndpointRateLimitAttribute>();

                if (endpointRateLimitAttribute is null)
                {
                    if (previousOnRejectedHandle is not null)
                    {
                        await previousOnRejectedHandle(onRejectedContext, cancellationToken);
                    }

                    return;
                }

                RateLimitOptions rateLimitOptions = onRejectedContext.HttpContext.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;
                IErrorLocalizerService errorLocalizerService = onRejectedContext.HttpContext.RequestServices.GetRequiredService<IErrorLocalizerService>();

                CustomException customException = new(HttpStatusCode.TooManyRequests, rateLimitOptions.RateLimitInternalErrorCode, endpointRateLimitAttribute.MilliSeconds);

                // Only the first rejection pays for the lookup. A race here is harmless: the factory hands out the same logger for the same category
                logger ??= onRejectedContext.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Bacon.ApiUtilities.RateLimiting");
                LogRateLimitExceeded(logger, onRejectedContext.HttpContext.Request.Method, onRejectedContext.HttpContext.Request.Path, customException.InternalCode);

                await ErrorResponseWriterService.WriteAsync(onRejectedContext.HttpContext, new ErrorResult(customException, errorLocalizerService.GetResourceValue(customException.InternalCode.ToString(CultureInfo.InvariantCulture))));
            };
        });

        return new(apiExceptionHandlerBuilder.ServiceCollection);
    }

    #region Privates

    private static RateLimitPartition<string> CreateRateLimitPartition(HttpContext httpContext)
    {
        Endpoint? endpoint = httpContext.GetEndpoint();

        if (endpoint is null)
        {
            return RateLimitPartition.GetNoLimiter("none");
        }

        EndpointRateLimitAttribute? endpointRateLimitAttribute = endpoint.Metadata.GetMetadata<EndpointRateLimitAttribute>();

        if (endpointRateLimitAttribute is not { MilliSeconds: > 0 })
        {
            return RateLimitPartition.GetNoLimiter("none");
        }

        string partitionKey = $"ratelimit_{GetIdentityKey(httpContext)}|{GetActionKey(endpoint)}";

        return RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromMilliseconds(endpointRateLimitAttribute.MilliSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }

    private static string GetIdentityKey(HttpContext httpContext)
    {
        ClaimsPrincipal claimsPrincipal = httpContext.User;

        return claimsPrincipal.FindFirst("sub")?.Value
                ?? claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("client_id")?.Value
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous";
    }

    private static string GetActionKey(Endpoint endpoint)
    {
        return _rateLimitCache.GetOrAdd(endpoint, static e =>
        {
            // Controller-Based Actions
            ControllerActionDescriptor? controllerActionDescriptor = e.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor != null)
            {
                return string.Concat(controllerActionDescriptor.ControllerTypeInfo.FullName, ":", controllerActionDescriptor.MethodInfo);
            }

            // Minimal APIs / Route-Based Endpoints
            if (e is RouteEndpoint { RoutePattern.RawText: { Length: > 0 } rawText })
            {
                string verbs = string.Join(",", e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? []);
                return string.Concat(verbs, ":", rawText);
            }

            // Fallback safely
            return e.DisplayName ?? string.Empty;
        });
    }

    #region Logs

    [LoggerMessage(Level = LogLevel.Warning, Message = "Rate limit exceeded while calling endpoint {Method}: {Path}. Error Code : {InternalCode}")]
    private static partial void LogRateLimitExceeded(ILogger logger, string method, string path, long internalCode);

    #endregion Logs

    #endregion Privates
}