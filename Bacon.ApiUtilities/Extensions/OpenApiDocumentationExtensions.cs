using Bacon.ApiUtilities.Filters;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Interfaces.Services.Apis.UI;
using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Bacon.ApiUtilities.Services.Apis;
using Bacon.ApiUtilities.Services.Apis.UI;
using Bacon.ApiUtilities.Services.Apis.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Globalization;

namespace Bacon.ApiUtilities.Extensions;

/// <summary>
/// OpenApi documentation extensions
/// </summary>
public static class OpenApiDocumentationExtensions
{
    private const string _openApiDocumentOutputCacheKey = "OpenApiDocumentsOutputCache";

    #region Service Middleware

    /// <summary>
    /// Add the open API documentation middleware
    /// </summary>
    /// <param name="apiExceptionHandlerBuilder">The middleware builder</param>
    /// <param name="configureOptions">The middleware options</param>
    public static ApiExceptionHandlerBuilder AddOpenApiDocumentation(this ApiExceptionHandlerBuilder apiExceptionHandlerBuilder, Action<OpenApiDocumentationOptions> configureOptions)
    {
        #region Options validation

        apiExceptionHandlerBuilder.ServiceCollection.AddOptions<OpenApiDocumentationOptions>()
            .Configure(configureOptions)
            .ValidateOnStart();

        apiExceptionHandlerBuilder.ServiceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<OpenApiDocumentationOptions>, OpenApiDocumentationOptionsValidatorService>());

        #endregion Options validation

        apiExceptionHandlerBuilder.ServiceCollection.TryAddSingleton<IEndpointMetadataService, EndpointMetadataService>();

        apiExceptionHandlerBuilder.ServiceCollection.TryAddKeyedTransient<IUiService, SwaggerUiService>(UiTypes.Swagger);
        apiExceptionHandlerBuilder.ServiceCollection.TryAddKeyedTransient<IUiService, ScalarUiService>(UiTypes.Scalar);

        apiExceptionHandlerBuilder.ServiceCollection.AddEndpointsApiExplorer();

        #region Output cache

        apiExceptionHandlerBuilder.ServiceCollection.AddOutputCache();

        apiExceptionHandlerBuilder.ServiceCollection.AddOptions<OutputCacheOptions>()
            .Configure<IOptions<OpenApiDocumentationOptions>>((outputCacheOptions, openApiDocumentationOptions) =>
            {
                TimeSpan duration = openApiDocumentationOptions.Value.OutputCacheDuration;

                outputCacheOptions.AddPolicy(_openApiDocumentOutputCacheKey, policy =>
                {
                    if (duration == TimeSpan.Zero)
                    {
                        policy.NoCache();
                        return;
                    }

                    policy.Expire(duration)
                            .VaryByValue(httpContext => new KeyValuePair<string, string>("uiculture", (httpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture ?? CultureInfo.CurrentUICulture).Name));
                });
            });

        #endregion Output cache

        #region Options snapshot

        OpenApiDocumentationOptions snapshot = new()
        {
            OpenApiDocumentInfos = []
        };

        configureOptions(snapshot);

        //creating a safe list of documents to register in case the options fails since openapi runs before start-up. The start-up will fail properly if the options are invalid
        IEnumerable<OpenApiDocumentInfo> documentsToRegister = (snapshot.OpenApiDocumentInfos ?? [])
            .Where(w => !string.IsNullOrWhiteSpace(w.Name))
            .DistinctBy(d => d.Name);

        #endregion Options snapshot

        #region Registering OpenApi documents

        foreach (OpenApiDocumentInfo openApiDocumentInfo in documentsToRegister)
        {
            apiExceptionHandlerBuilder.ServiceCollection.AddOpenApi(openApiDocumentInfo.Name, o =>
            {
                // Modify the base document metadata (Title, Version, Contact Info)
                o.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = openApiDocumentInfo.Title,
                        Version = openApiDocumentInfo.Version,
                        Description = openApiDocumentInfo.Description,
                        License = openApiDocumentInfo.License
                    };

                    document.Servers = null;

                    return Task.CompletedTask;
                });

                o.AddDocumentTransformer(new OpenApiDocumentFilter(openApiDocumentInfo.Name));
            });
        }

        #endregion Registering OpenApi documents

        return new(apiExceptionHandlerBuilder.ServiceCollection);
    }

    #endregion Service Middleware

    #region App Middleware

    /// <summary>
    /// Use the open API documentation middleware. webApplication.UseOutputCache() must be used for the output cache to work as per the README
    /// </summary>
    /// <param name="webApplication">The webapplication</param>
    public static WebApplication UseOpenApiDocumentation(this WebApplication webApplication)
    {
        OpenApiDocumentationOptions openApiDocumentationOptions = webApplication.Services.GetRequiredService<IOptions<OpenApiDocumentationOptions>>().Value;

        webApplication.MapOpenApi().CacheOutput(_openApiDocumentOutputCacheKey);

        IUiService uiService = webApplication.Services.GetRequiredKeyedService<IUiService>(openApiDocumentationOptions.UiConfigs.UiType);

        uiService.UseUI(webApplication);

        return webApplication;
    }

    #endregion App Middleware
}