using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Bacon.ApiUtilities.Services;
using Bacon.ApiUtilities.Services.Apis;
using Bacon.ApiUtilities.Services.Apis.Validators;
using Bacon.ApiUtilities.Services.ModelStateValidations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Bacon.ApiUtilities.Extensions;

/// <summary>
/// Api Exception handler extensions
/// </summary>
public static class ApiExceptionHandlerExtensions
{
    #region Publics

    #region Service extension

    /// <summary>
    /// Add model state validation with custom error localizer information.
    /// </summary>
    /// <typeparam name="TResource">Custom error resource type</typeparam>
    /// <param name="mvcBuilder">IMvcBuilder</param>
    /// <param name="configureOptions">ModelStateValidationOptions action</param>
    /// <returns>ApiExceptionHandlerBuilder</returns>
    public static ApiExceptionHandlerBuilder AddApiExceptionHandler<TResource>(this IMvcBuilder mvcBuilder, Action<ModelStateValidationOptions> configureOptions) where TResource : class
    {
        mvcBuilder.Services.AddOptions<ModelStateValidationOptions>()
            .Configure(configureOptions)
            .ValidateOnStart();

        mvcBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ModelStateValidationOptions>, ModelStateValidationOptionsValidatorService>());

        mvcBuilder.Services.AddLocalization();

        mvcBuilder.Services.TryAddTransient<IModelValidationService, ModelValidationService>();
        mvcBuilder.Services.TryAddSingleton<IErrorLocalizerService, ErrorLocalizerService<TResource>>();

        mvcBuilder.AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.AllowTrailingCommas = true;
            o.JsonSerializerOptions.AllowDuplicateProperties = false;
        });

        mvcBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ApiBehaviorOptions>, ModelStateApiBehaviorOptionsService>());

        mvcBuilder.Services.AddExceptionHandler<ApiExceptionHandler>();

        return new(mvcBuilder.Services);
    }

    #endregion Service extension

    #region Middleware extension

    /// <summary>
    /// Use custom exception handling middleware
    /// </summary>
    /// <param name="applicationBuilder">IApplicationBuilder</param>
    public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder applicationBuilder)
    {
        // The handler always returns true, so this delegate is never reached. It only exists so UseExceptionHandler does not require the consumer to also register AddProblemDetails
        return applicationBuilder.UseExceptionHandler(static _ => { });
    }

    #endregion Middleware extension

    #endregion Publics
}