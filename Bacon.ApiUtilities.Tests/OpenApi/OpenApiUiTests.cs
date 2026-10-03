using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Tests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Bacon.ApiUtilities.Tests.OpenApi;

/// <summary>
/// The UI is chosen by <see cref="UiTypes"/> and served under the virtual path.
/// Swagger and Scalar do not treat the path the same way, so the library must hide the difference
/// </summary>
[TestFixture]
internal sealed class OpenApiUiTests
{
    private const string DocumentName = "uidoc";

    private readonly List<WebApplication> _webApplications = [];

    [TearDown]
    public async Task TearDown()
    {
        foreach (WebApplication webApplication in _webApplications)
        {
            await webApplication.StopAsync();
            await webApplication.DisposeAsync();
        }

        _webApplications.Clear();
    }

    #region Swagger

    [TestCase("", "")]
    [TestCase("docs", "docs")]
    [TestCase("/docs", "docs")]
    [TestCase("docs/", "docs")]
    [TestCase("/docs/", "docs")]
    [TestCase("api/docs", "api/docs")]
    public async Task Swagger_ShouldServeTheUiUnderTheNormalizedVirtualPath(string virtualPath, string servedPath)
    {
        HttpClient client = await StartAsync(UiTypes.Swagger, virtualPath);

        using HttpResponseMessage response = await client.GetAsync(BuildUrl(servedPath, "index.html"));
        string body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("swagger").IgnoreCase);
        });
    }

    [Test]
    public async Task Swagger_ShouldListTheDocumentEndpoint()
    {
        HttpClient client = await StartAsync(UiTypes.Swagger, "docs");

        // The Swagger UI configuration, including the document list, is served by index.js
        using HttpResponseMessage response = await client.GetAsync("/docs/index.js");

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain($"/openapi/{DocumentName}.json"));
        });
    }

    #endregion Swagger

    #region Scalar

    [TestCase("", "")]
    [TestCase("docs", "docs")]
    [TestCase("/docs", "docs")]
    [TestCase("docs/", "docs")]
    [TestCase("api/docs", "api/docs")]
    public async Task Scalar_ShouldServeTheUiUnderTheVirtualPath(string virtualPath, string servedPath)
    {
        HttpClient client = await StartAsync(UiTypes.Scalar, virtualPath);

        using HttpResponseMessage response = await client.GetAsync(BuildUrl(servedPath, DocumentName));
        string body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("scalar").IgnoreCase);
        });
    }

    #endregion Scalar

    #region UI selection

    [Test]
    public async Task UiType_ShouldSelectTheMatchingUi()
    {
        HttpClient swagger = await StartAsync(UiTypes.Swagger, "docs");
        HttpClient scalar = await StartAsync(UiTypes.Scalar, "docs");

        using HttpResponseMessage swaggerPage = await swagger.GetAsync("/docs/index.html");
        using HttpResponseMessage scalarPage = await scalar.GetAsync($"/docs/{DocumentName}");

        string swaggerBody = await swaggerPage.Content.ReadAsStringAsync();
        string scalarBody = await scalarPage.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(swaggerPage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(scalarPage.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Each page is recognisable, and neither one contains the other UI's marker, so each type really selects its own service
            Assert.That(swaggerBody, Does.Contain("swagger-ui").IgnoreCase);
            Assert.That(swaggerBody, Does.Not.Contain("scalar").IgnoreCase);
            Assert.That(scalarBody, Does.Contain("scalar").IgnoreCase);
            Assert.That(scalarBody, Does.Not.Contain("swagger-ui").IgnoreCase);
        });
    }

    /// <summary>
    /// The validator only knows the enum. Every member must have a registered UI service,
    /// or the options validate and the application then fails when the UI is resolved
    /// </summary>
    [Test]
    public async Task EveryUiType_ShouldHaveARegisteredUiService([ValueSource(nameof(AllUiTypes))] UiTypes uiType)
    {
        HttpClient client = await StartAsync(uiType, "docs");

        using HttpResponseMessage response = await client.GetAsync($"/openapi/{DocumentName}.json");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private static IEnumerable<UiTypes> AllUiTypes => Enum.GetValues<UiTypes>();

    [Test]
    public async Task UiType_ShouldKeepServingTheOpenApiDocument()
    {
        HttpClient client = await StartAsync(UiTypes.Scalar, "docs");

        using HttpResponseMessage response = await client.GetAsync($"/openapi/{DocumentName}.json");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    #endregion UI selection

    #region Invalid configuration

    [Test]
    public void InvalidVirtualPath_ShouldFailAtStartupForEveryUi([Values(UiTypes.Swagger, UiTypes.Scalar)] UiTypes uiType, [Values("my docs", "docs?x=1", "a//b", "../x")] string virtualPath)
    {
        OptionsValidationException? exception = Assert.ThrowsAsync<OptionsValidationException>(async () => await StartAsync(uiType, virtualPath));

        Assert.That(string.Join(";", exception!.Failures), Does.Contain("virtual path"));
    }

    #endregion Invalid configuration

    #region Helpers

    private static string BuildUrl(string virtualPath, string leaf)
    {
        return "/" + (virtualPath + "/" + leaf).Trim('/');
    }

    private async Task<HttpClient> StartAsync(UiTypes uiType, string virtualPath)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddSingleton<IErrorLocalizerService, TestErrorLocalizerService>();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(OpenApiTestController).Assembly)
            .AddApiExceptionHandler<TestResources>(o =>
            {
                o.InternalServerErrorInternalErrorCode = 500000;
                o.UnhandledValidationInternalErrorCode = 412000;
            })
            .AddOpenApiDocumentation(o =>
            {
                o.OpenApiDocumentInfos = [new() { Name = DocumentName, Title = "UI document", Version = "1" }];
                o.UiConfigs.UiType = uiType;
                o.UiConfigs.VirtualPath = virtualPath;
            });

        WebApplication webApplication = builder.Build();

        _webApplications.Add(webApplication);

        webApplication.UseApiExceptionHandler();
        webApplication.UseOutputCache();
        webApplication.UseOpenApiDocumentation();
        webApplication.MapControllers();

        await webApplication.StartAsync();

        return webApplication.GetTestClient();
    }

    #endregion Helpers
}
