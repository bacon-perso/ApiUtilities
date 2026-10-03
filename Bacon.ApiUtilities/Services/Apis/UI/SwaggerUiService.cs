using Bacon.ApiUtilities.Interfaces.Services.Apis.UI;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Bacon.ApiUtilities.Services.Apis.UI;

internal sealed class SwaggerUiService (IOptions<OpenApiDocumentationOptions> configureOptions) : IUiService
{
    private const string _embeddedFileName = "UI.js.swagger.custom.js";

    public void UseUI(WebApplication webApplication)
    {
        OpenApiDocumentationOptions openApiDocumentationOptions = configureOptions.Value;

        webApplication.UseSwaggerUI(o =>
        {
            foreach (OpenApiDocumentInfo openApiDocumentInfo in openApiDocumentationOptions.OpenApiDocumentInfos)
            {
                o.SwaggerEndpoint($"/openapi/{openApiDocumentInfo.Name}.json", openApiDocumentInfo.Title);
            }

            o.RoutePrefix = openApiDocumentationOptions.UiConfigs.NormalizedVirtualPath;
            o.SupportedSubmitMethods(openApiDocumentationOptions.UiConfigs.SupportedSubmitMethods ?? []);
            o.DocExpansion(openApiDocumentationOptions.UiConfigs.CollapseEndpoints ? Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None : Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            o.ShowExtensions();
            o.DefaultModelsExpandDepth(-1);

            o.InjectJavascript(InjectJavascriptFile());

            o.HeadContent = GetCustomCss(new(o.HeadContent), openApiDocumentationOptions.UiConfigs.CorsPolicyCss);
        });
    }

    #region Private

    #region Get CSS

    private static string GetCustomCss(StringBuilder stringBuilder, Dictionary<string, string>? corsPolicyCss)
    {
        stringBuilder.AppendLine("<style>");
        stringBuilder.AppendLine(".swagger-ui .opblock .opblock-summary-path__deprecated { text-decoration: line-through; }");
        stringBuilder.AppendLine(".swagger-ui .opblock .opblock-summary-description { -webkit-box-flex: 1; -ms-flex: 1; flex: 1; }");
        stringBuilder.AppendLine("small.privacy-stamp { margin-right: 15px; float:right; } ");
        stringBuilder.AppendLine("small.privacy-stamp pre.privacy { padding: 2px 4px; border-radius: 57px; font-size: 14px; color: #fff; font-weight: bold; font-family: Titillium Web, sans-serif; margin: 0;}");
        stringBuilder.AppendLine("small.privacy-stamp pre.privacy.public { background-color: #2a9f8a; }");
        stringBuilder.AppendLine("small.privacy-stamp pre.privacy.private { background-color: #eb343a; }");

        if (corsPolicyCss?.Count > 0)
        {
            foreach ((string corsPolicyName, string hexColor) in corsPolicyCss)
            {
                stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"small.privacy-stamp pre.privacy.{corsPolicyName.ToLower(CultureInfo.InvariantCulture).Replace(" ", "-")} {{ background-color: {hexColor}; }}");
            }
        }

        stringBuilder.AppendLine("</style>");

        return stringBuilder.ToString();
    }

    #endregion Get CSS

    #region Inject Javascript

    private static string InjectJavascriptFile()
    {
        Assembly assembly = typeof(SwaggerUiService).Assembly;

        string embeddedFile = $"{assembly.GetName().Name}.{_embeddedFileName}";

        using Stream stream = assembly.GetManifestResourceStream(embeddedFile) ?? throw new IOException($"The embedded file {embeddedFile} cannot be loaded into the application");

        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);

        string base64 = Convert.ToBase64String(memoryStream.GetBuffer().AsSpan(0, (int)memoryStream.Length));

        return $"data:text/javascript;base64,{base64}";
    }

    #endregion Inject Javascript

    #endregion Private
}