using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models.Apis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bacon.ApiUtilities.Tests.Support;

/// <summary>
/// Registers the library the way an application does, with the internal error codes chosen by the test.
/// The rate limit code is only registered when a value is given.
/// </summary>
internal static class OptionsTestSetup
{
    /// <summary>
    /// A provider only. The options are validated when they are first resolved
    /// </summary>
    public static ServiceProvider BuildProvider(long unhandledValidationCode, long internalServerErrorCode, long? rateLimitCode = null)
    {
        ServiceCollection services = new();

        services.AddLogging();

        Register(services.AddControllers(), unhandledValidationCode, internalServerErrorCode, rateLimitCode);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// A host that is not started. Starting it runs the validations registered with ValidateOnStart
    /// </summary>
    public static WebApplication BuildWebApplication(long unhandledValidationCode, long internalServerErrorCode, long? rateLimitCode = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<IErrorLocalizerService, TestErrorLocalizerService>();

        Register(builder.Services.AddControllers(), unhandledValidationCode, internalServerErrorCode, rateLimitCode);

        return builder.Build();
    }

    private static void Register(IMvcBuilder mvcBuilder, long unhandledValidationCode, long internalServerErrorCode, long? rateLimitCode)
    {
        ApiExceptionHandlerBuilder apiExceptionHandlerBuilder = mvcBuilder.AddApiExceptionHandler<TestResources>(o =>
        {
            o.UnhandledValidationInternalErrorCode = unhandledValidationCode;
            o.InternalServerErrorInternalErrorCode = internalServerErrorCode;
        });

        if (rateLimitCode.HasValue)
        {
            apiExceptionHandlerBuilder.AddApiRateLimiting(o =>
            {
                o.RateLimitInternalErrorCode = rateLimitCode.Value;
            });
        }
    }
}
