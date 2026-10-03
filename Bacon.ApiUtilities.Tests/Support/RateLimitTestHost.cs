using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace Bacon.ApiUtilities.Tests.Support;

/// <summary>
/// Where <c>UseApiExceptionHandler</c> is registered relative to <c>UseRateLimiter</c>
/// </summary>
internal enum ExceptionHandlerPlacement
{
    BeforeRateLimiter,
    AfterRateLimiter,
    NotUsed
}

/// <summary>
/// In-memory host using the library the way an application does, with the middleware order documented in the README.
/// Every host has its own rate limiter state, so tests do not influence each other.
/// </summary>
internal sealed class RateLimitTestHost : IAsyncDisposable
{
    /// <summary>
    /// Deliberately not 429000, to prove the configured value is used and not a hardcoded one
    /// </summary>
    public const long RateLimitInternalErrorCode = 429001;

    public const string UserHeaderName = "X-Test-User";

    private readonly WebApplication _webApplication;

    public HttpClient Client { get; }

    private RateLimitTestHost(WebApplication webApplication)
    {
        _webApplication = webApplication;
        Client = webApplication.GetTestClient();
    }

    /// <param name="configureConsumerServices">Registers the application's own services before the library, e.g. its own rate limiting</param>
    /// <param name="exceptionHandlerPlacement">Where UseApiExceptionHandler is placed relative to UseRateLimiter. Defaults to the order documented in the README</param>
    /// <param name="configureLogging">Adds logging providers, e.g. to capture the logs</param>
    public static async Task<RateLimitTestHost> StartAsync(Action<IServiceCollection>? configureConsumerServices = null, ExceptionHandlerPlacement exceptionHandlerPlacement = ExceptionHandlerPlacement.BeforeRateLimiter, Action<ILoggingBuilder>? configureLogging = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        configureLogging?.Invoke(builder.Logging);

        // Registered before AddApiExceptionHandler, which only adds its own localizer when none is registered
        builder.Services.AddSingleton<IErrorLocalizerService, TestErrorLocalizerService>();

        configureConsumerServices?.Invoke(builder.Services);

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(RateLimitTestController).Assembly)
            .AddApiExceptionHandler<TestResources>(o =>
            {
                o.InternalServerErrorInternalErrorCode = 500000;
                o.UnhandledValidationInternalErrorCode = 412000;
            })
            .AddApiRateLimiting(o =>
            {
                o.RateLimitInternalErrorCode = RateLimitInternalErrorCode;
            });

        WebApplication webApplication = builder.Build();

        webApplication.UseRouting();

        if (exceptionHandlerPlacement == ExceptionHandlerPlacement.BeforeRateLimiter)
        {
            webApplication.UseApiExceptionHandler();
        }

        // Stands in for UseAuthentication: identifies the caller with a "sub" claim taken from a header
        webApplication.Use((httpContext, next) =>
        {
            if (httpContext.Request.Headers.TryGetValue(UserHeaderName, out StringValues user))
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", user.ToString())], "Test"));
            }

            return next(httpContext);
        });

        webApplication.UseRateLimiter();

        if (exceptionHandlerPlacement == ExceptionHandlerPlacement.AfterRateLimiter)
        {
            webApplication.UseApiExceptionHandler();
        }

        webApplication.MapControllers();

        await webApplication.StartAsync();

        return new(webApplication);
    }

    public Task<HttpResponseMessage> GetAsync(string path, string? user = null)
    {
        HttpRequestMessage request = new(HttpMethod.Get, path);

        if (user != null)
        {
            request.Headers.Add(UserHeaderName, user);
        }

        return Client.SendAsync(request);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();

        await _webApplication.StopAsync();
        await _webApplication.DisposeAsync();
    }
}
