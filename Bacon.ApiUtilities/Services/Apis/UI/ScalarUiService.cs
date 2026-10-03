using Bacon.ApiUtilities.Interfaces.Services.Apis.UI;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

namespace Bacon.ApiUtilities.Services.Apis.UI;

internal sealed class ScalarUiService(IOptions<OpenApiDocumentationOptions> configureOptions) : IUiService
{
    public void UseUI(WebApplication webApplication)
    {
        OpenApiDocumentationOptions openApiDocumentationOptions = configureOptions.Value;

        webApplication.MapScalarApiReference(openApiDocumentationOptions.UiConfigs.NormalizedVirtualPath, o =>
        {
            o.HideModels = true;
            o.ShowOperationId = true;
            o.HideTestRequestButton = true;

            o.ShowDeveloperTools = DeveloperToolsVisibility.Never;

            o.DocumentDownloadType = DocumentDownloadType.Direct;

            if (!openApiDocumentationOptions.UiConfigs.CollapseEndpoints)
            {
                o.ExpandAllTags();
            }

            foreach (OpenApiDocumentInfo openApiDocumentInfo in openApiDocumentationOptions.OpenApiDocumentInfos)
            {
                o.AddDocument(openApiDocumentInfo.Name, openApiDocumentInfo.Title);
            }
        });
    }
}