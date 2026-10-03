using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.RegularExpressions;

namespace Bacon.ApiUtilities.Services.Apis.Validators;

internal sealed partial class OpenApiDocumentationOptionsValidatorService : IValidateOptions<OpenApiDocumentationOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenApiDocumentationOptions openApiDocumentationOptions)
    {
        List<string> errors = [];

        ValidateOpenApiDocumentInfos(openApiDocumentationOptions, errors);
        ValidateOutputCacheDuration(openApiDocumentationOptions, errors);
        ValidateUiConfigs(openApiDocumentationOptions, errors);

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private static void ValidateOpenApiDocumentInfos(OpenApiDocumentationOptions openApiDocumentationOptions, List<string> errors)
    {
        if (openApiDocumentationOptions.OpenApiDocumentInfos == null || !openApiDocumentationOptions.OpenApiDocumentInfos.Any())
        {
            errors.Add("Invalid empty OpenAPI document info list.");
            return;
        }

        if (openApiDocumentationOptions.OpenApiDocumentInfos.Any(a => string.IsNullOrWhiteSpace(a.Title)))
        {
            errors.Add($"Found invalid OpenAPI doc info with empty title.");
        }

        if (openApiDocumentationOptions.OpenApiDocumentInfos.Any(a => string.IsNullOrWhiteSpace(a.Name)))
        {
            errors.Add($"Found invalid OpenAPI doc info with empty name.");
        }

        string[] duplicatedNames = [.. openApiDocumentationOptions.OpenApiDocumentInfos.GroupBy(g => g.Name).Where(w => w.Count() > 1).Select(s => s.Key)];
        if (duplicatedNames.Length != 0)
        {
            errors.Add($"The following OpenAPI document names are duplicated: {string.Join(", ", duplicatedNames)}");
        }

        string[] invalidNames = [.. openApiDocumentationOptions.OpenApiDocumentInfos.Where(a => a.Name.Any(char.IsWhiteSpace)).Select(s => s.Name)];
        if (invalidNames.Length != 0)
        {
            errors.Add($"Found invalid OpenAPI doc info with empty space names: {string.Join(", ", invalidNames)}");
        }

        if (openApiDocumentationOptions.OpenApiDocumentInfos.Any(a => string.IsNullOrWhiteSpace(a.Version)))
        {
            errors.Add($"Found invalid OpenAPI doc info with empty version.");
        }
    }

    private static void ValidateOutputCacheDuration(OpenApiDocumentationOptions openApiDocumentationOptions, List<string> errors)
    {
        if (openApiDocumentationOptions.OutputCacheDuration < TimeSpan.Zero)
        {
            errors.Add("The output cache duration cannot be lower than 0");
        }
    }

    private static void ValidateUiConfigs(OpenApiDocumentationOptions openApiDocumentationOptions, List<string> errors)
    {
        if (openApiDocumentationOptions.UiConfigs is null)
        {
            errors.Add("The UI configs cannot be null");
            return;
        }

        if (!Enum.IsDefined(openApiDocumentationOptions.UiConfigs.UiType))
        {
            errors.Add($"Invalid UI type '{(int)openApiDocumentationOptions.UiConfigs.UiType}'. Supported values: {string.Join(", ", Enum.GetNames<UiTypes>())}");
        }

        if (!VirtualPathRegex().IsMatch(openApiDocumentationOptions.UiConfigs.NormalizedVirtualPath) || openApiDocumentationOptions.UiConfigs.NormalizedVirtualPath.Split('/').Any(a => a is "." or ".."))
        {
            errors.Add($"Invalid UI virtual path '{openApiDocumentationOptions.UiConfigs.VirtualPath}'. Use URL-safe segments separated by single slashes, for example 'docs' or 'api/docs'");
        }

        if (openApiDocumentationOptions.UiConfigs.SupportedSubmitMethods != null)
        {
            if (!Utilities.Validators.TryValidateDuplicatedItems(openApiDocumentationOptions.UiConfigs.SupportedSubmitMethods, out IEnumerable<SubmitMethod>? invalidSubmitMethods))
            {
                errors.Add($"Invalid duplicated submit methods: {string.Join(", ", invalidSubmitMethods)}");
            }
        }

        if (openApiDocumentationOptions.UiConfigs.CorsPolicyCss != null)
        {
            if (openApiDocumentationOptions.UiConfigs.CorsPolicyCss.Any(w => string.IsNullOrWhiteSpace(w.Key)))
            {
                errors.Add("Found invalid empty cors policy names.");
            }

            IEnumerable<string> invalidHexColors = openApiDocumentationOptions.UiConfigs.CorsPolicyCss.Where(w => !IsValidCssColorFormat(w.Value)).Select(s => s.Value);
            if (invalidHexColors.Any())
            {
                errors.Add($"The following Hex colors are invalid: {string.Join(", ", invalidHexColors)}");
            }
        }
    }

    private static bool IsValidCssColorFormat(string hexColorCode) => ColorHexRegex().IsMatch(hexColorCode);

    //Regex from http://stackoverflow.com/a/1636354/2343
    [GeneratedRegex("^#(?:[0-9a-fA-F]{3}){1,2}$")]
    private static partial Regex ColorHexRegex();

    //Normalized virtual path check
    [GeneratedRegex(@"^([A-Za-z0-9._~-]+(/[A-Za-z0-9._~-]+)*)?$")]
    private static partial Regex VirtualPathRegex();
}