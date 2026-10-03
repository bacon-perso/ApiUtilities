using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Bacon.ApiUtilities.Tests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bacon.ApiUtilities.Tests.OpenApi;

/// <summary>
/// Generates the real OpenAPI document through the library and checks what the document transformer produced
/// </summary>
[TestFixture]
internal sealed class OpenApiDocumentFilterTests
{
    private const string AnonymousPath = "/openapitest/anonymous";
    private const string PlainPath = "/openapitest/plain";
    private const string LimitedPath = "/openapitest/limited";
    private const string HiddenPath = "/openapitest/hidden";
    private const string DeprecatedPath = "/openapitest/deprecated";
    private const string HiddenOnlyPath = "/openapitest-hiddenonly/secret";
    private const string MinimalApiPath = "/openapitest/minimal";
    private const string SharedRoutePath = "/openapitest-shared";
    private const long RateLimitCode = 429001;

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

    #region Paths and documents

    [Test]
    public async Task Document_ShouldOnlyContainEndpointsOfItsOwnDefinition()
    {
        using JsonDocument document = await GetDocumentAsync();

        List<string> paths = [.. document.RootElement.GetProperty("paths").EnumerateObject().Select(s => s.Name)];

        Assert.That(paths, Is.EquivalentTo([PlainPath, AnonymousPath, LimitedPath, DeprecatedPath, SharedRoutePath]));
    }

    [Test]
    public async Task SkippedEndpoint_ShouldNotBeInAnyDocument([Values(OpenApiTestControllers.DocumentName, OpenApiTestControllers.OtherDocumentName)] string documentName, [Values] bool displayHiddenEndpoints)
    {
        using JsonDocument document = await GetDocumentAsync(documentName, displayHiddenEndpoints);

        Assert.That(document.RootElement.GetProperty("paths").TryGetProperty(MinimalApiPath, out _), Is.False);
    }

    [Test]
    public async Task OtherDocument_ShouldOnlyContainItsOwnEndpoints()
    {
        using JsonDocument document = await GetDocumentAsync(OpenApiTestControllers.OtherDocumentName);

        List<string> paths = [.. document.RootElement.GetProperty("paths").EnumerateObject().Select(s => s.Name)];

        Assert.That(paths, Is.EquivalentTo(["/openapitest-other/item"]));
    }

    [Test]
    public async Task HiddenEndpoint_WhenDisplayHiddenEndpointsIsFalse_ShouldBeRemoved()
    {
        using JsonDocument document = await GetDocumentAsync(displayHiddenEndpoints: false);

        Assert.That(document.RootElement.GetProperty("paths").TryGetProperty(HiddenPath, out _), Is.False);
    }

    [Test]
    public async Task HiddenEndpoint_WhenDisplayHiddenEndpointsIsTrue_ShouldBeDocumentedAsPrivate()
    {
        using JsonDocument document = await GetDocumentAsync(displayHiddenEndpoints: true);

        JsonElement accessRights = GetOperation(document, HiddenPath).GetProperty("x-access-rights");

        Assert.That(accessRights.GetProperty("privacy").GetString(), Is.EqualTo("Private"));
    }

    #endregion Paths and documents

    #region Operation

    [Test]
    public async Task Operation_ShouldHaveOperationIdTagAndDeprecation()
    {
        using JsonDocument document = await GetDocumentAsync();

        JsonElement plain = GetOperation(document, PlainPath);
        JsonElement deprecated = GetOperation(document, DeprecatedPath);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(plain.GetProperty("operationId").GetString(), Is.EqualTo("Plain"));
            Assert.That(plain.GetProperty("tags").EnumerateArray().Select(s => s.GetString()), Is.EqualTo([OpenApiTestControllers.TagName]));
            Assert.That(deprecated.GetProperty("deprecated").GetBoolean(), Is.True);
            Assert.That(plain.TryGetProperty("deprecated", out JsonElement plainDeprecated) && plainDeprecated.GetBoolean(), Is.False);
        }
    }

    [Test]
    public async Task Document_ShouldOnlyListTheTagsOfItsEndpoints()
    {
        using JsonDocument document = await GetDocumentAsync();

        IEnumerable<string?> tags = document.RootElement.GetProperty("tags").EnumerateArray().Select(s => s.GetProperty("name").GetString());

        Assert.That(tags, Is.EqualTo([OpenApiTestControllers.TagName]));
    }

    #endregion Operation

    #region Responses

    [Test]
    public async Task Operation_ShouldDocumentErrorResponsesGroupedByHttpStatusCode()
    {
        using JsonDocument document = await GetDocumentAsync();

        JsonElement responses = GetOperation(document, LimitedPath).GetProperty("responses");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(responses.GetProperty("500").GetProperty("description").GetString(), Does.Contain("* 500000 :"));
            Assert.That(responses.GetProperty("412").GetProperty("description").GetString(), Does.Contain("* 412000 :"));
            Assert.That(responses.GetProperty("409").GetProperty("description").GetString(), Does.Contain($"* {OpenApiTestControllers.CustomErrorCode} :"));
            Assert.That(responses.GetProperty("429").GetProperty("description").GetString(), Does.Contain($"* {RateLimitCode} :"));
        }
    }

    [Test]
    public async Task Operation_WithoutRateLimitOrCustomCodes_ShouldNotDocumentThem()
    {
        using JsonDocument document = await GetDocumentAsync();

        JsonElement responses = GetOperation(document, PlainPath).GetProperty("responses");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(responses.TryGetProperty("429", out _), Is.False);
            Assert.That(responses.TryGetProperty("409", out _), Is.False);
        }
    }

    [TestCase("/openapitest-ratelimit/zero")]
    [TestCase("/openapitest-ratelimit/unset")]
    public async Task Operation_WithZeroMilliSeconds_ShouldNotDocumentTheRateLimit(string path)
    {
        using JsonDocument document = await GetDocumentAsync(OpenApiTestControllers.RateLimitDocumentName);

        JsonElement operation = GetOperation(document, path);

        using (Assert.EnterMultipleScope())
        {
            // The endpoint is not rate limited, so it can never answer with the rate limit error
            Assert.That(operation.GetProperty("responses").TryGetProperty("429", out _), Is.False);
            Assert.That(operation.TryGetProperty("x-ratelimit", out _), Is.False);
        }
    }

    [Test]
    public async Task Operation_WithMilliSecondsAboveZero_ShouldDocumentTheRateLimit()
    {
        using JsonDocument document = await GetDocumentAsync(OpenApiTestControllers.RateLimitDocumentName);

        JsonElement operation = GetOperation(document, "/openapitest-ratelimit/limited");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(operation.GetProperty("responses").GetProperty("429").GetProperty("description").GetString(), Does.Contain($"* {RateLimitCode} :"));
            Assert.That(operation.GetProperty("x-ratelimit").GetProperty("time-in-milliseconds").GetInt32(), Is.EqualTo(OpenApiTestControllers.RateLimitMilliSeconds));
        }
    }

    #endregion Responses

    #region Extensions

    [Test]
    public async Task Operation_ShouldDocumentAccessRights()
    {
        using JsonDocument document = await GetDocumentAsync();

        JsonElement anonymous = GetOperation(document, AnonymousPath).GetProperty("x-access-rights");
        JsonElement plain = GetOperation(document, PlainPath).GetProperty("x-access-rights");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(anonymous.GetProperty("privileges").GetString(), Is.EqualTo("Anonymous"));
            Assert.That(anonymous.GetProperty("privacy").GetString(), Is.EqualTo("Public"));
            Assert.That(plain.GetProperty("privileges").GetString(), Is.Empty);
        }
    }

    [Test]
    public async Task Operation_ShouldOnlyDocumentTheRateLimitWhenThereIsOne()
    {
        using JsonDocument document = await GetDocumentAsync();

        JsonElement limited = GetOperation(document, LimitedPath);
        JsonElement plain = GetOperation(document, PlainPath);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(limited.GetProperty("x-ratelimit").GetProperty("time-in-milliseconds").GetInt32(), Is.EqualTo(OpenApiTestControllers.RateLimitMilliSeconds));
            Assert.That(plain.TryGetProperty("x-ratelimit", out _), Is.False);
        }
    }

    /// <summary>
    /// An operation transformer of the application runs before the library document transformer and already sets the extensions.
    /// The library must replace them and not fail on the existing keys.
    /// </summary>
    [Test]
    public async Task Operation_WithExistingExtensions_ShouldOverwriteThemWithoutFailing()
    {
        using JsonDocument document = await GetDocumentAsync(addStaleExtensions: true);

        JsonElement limited = GetOperation(document, LimitedPath);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(limited.GetProperty("x-access-rights").GetProperty("privacy").GetString(), Is.EqualTo("Public"));
            Assert.That(limited.GetProperty("x-ratelimit").GetProperty("time-in-milliseconds").GetInt32(), Is.EqualTo(OpenApiTestControllers.RateLimitMilliSeconds));
        }
    }

    #endregion Extensions

    #region Schemas

    [Test]
    public async Task Components_ShouldOnlyContainTheSchemasOfItsOwnEndpointsAndTheErrorResult()
    {
        using JsonDocument document = await GetDocumentAsync();

        List<string> schemas = [.. document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject().Select(s => s.Name)];

        Assert.That(schemas, Is.EquivalentTo(["ErrorResult", nameof(WidgetDto)]));
    }

    [Test]
    public async Task Components_WithGenericResponseTypes_ShouldHaveOneSchemaPerClosedType()
    {
        using JsonDocument document = await GetDocumentAsync(OpenApiTestControllers.GenericDocumentName);

        List<string> schemas = [.. document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject().Select(s => s.Name)];
        List<string> listSchemas = [.. schemas.Where(w => w.StartsWith("ListReturnData", StringComparison.Ordinal))];

        using (Assert.EnterMultipleScope())
        {
            // ListReturnData<A> and ListReturnData<B> must not collapse into a single raw CLR name such as ListReturnData`1
            Assert.That(listSchemas, Has.Count.EqualTo(2));
            Assert.That(schemas, Has.None.Contains("`"));
            Assert.That(schemas, Does.Contain(nameof(GenericItemADto)));
            Assert.That(schemas, Does.Contain(nameof(GenericItemBDto)));
        }
    }

    /// <summary>
    /// Every $ref of the document must point to an existing schema. Guards against schemas stored under a name that differs from the one the operations reference
    /// </summary>
    [Test]
    public async Task Document_EverySchemaReference_ShouldResolveToAnExistingSchema([Values(OpenApiTestControllers.DocumentName, OpenApiTestControllers.OtherDocumentName, OpenApiTestControllers.GenericDocumentName)] string documentName)
    {
        using JsonDocument document = await GetDocumentAsync(documentName);

        HashSet<string> schemas = [.. document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject().Select(s => s.Name)];
        List<string> references = [];

        CollectSchemaReferences(document.RootElement, references);

        List<string> unresolvedReferences = [.. references.Where(w => !schemas.Contains(w)).Distinct()];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(references, Is.Not.Empty, "The document has no schema reference, the test would prove nothing");
            Assert.That(unresolvedReferences, Is.Empty);
        }
    }

    #endregion Schemas

    #region Hidden endpoints with their own tag and schema

    [Test]
    public async Task HiddenOnlyTagAndSchema_WhenDisplayHiddenEndpointsIsFalse_ShouldNotRemainInTheDocument()
    {
        using JsonDocument document = await GetDocumentAsync(displayHiddenEndpoints: false);

        IEnumerable<string?> tags = document.RootElement.GetProperty("tags").EnumerateArray().Select(s => s.GetProperty("name").GetString());
        List<string> schemas = [.. document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject().Select(s => s.Name)];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(document.RootElement.GetProperty("paths").TryGetProperty(HiddenOnlyPath, out _), Is.False);
            Assert.That(tags, Does.Not.Contain(OpenApiTestControllers.HiddenOnlyTagName));
            Assert.That(schemas, Does.Not.Contain(nameof(HiddenOnlyDto)));
        }
    }

    [Test]
    public async Task HiddenOnlyTagAndSchema_WhenDisplayHiddenEndpointsIsTrue_ShouldBeInTheDocument()
    {
        using JsonDocument document = await GetDocumentAsync(displayHiddenEndpoints: true);

        IEnumerable<string?> tags = document.RootElement.GetProperty("tags").EnumerateArray().Select(s => s.GetProperty("name").GetString());
        List<string> schemas = [.. document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject().Select(s => s.Name)];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(document.RootElement.GetProperty("paths").TryGetProperty(HiddenOnlyPath, out _), Is.True);
            Assert.That(tags, Does.Contain(OpenApiTestControllers.HiddenOnlyTagName));
            Assert.That(schemas, Does.Contain(nameof(HiddenOnlyDto)));
        }
    }

    #endregion Hidden endpoints with their own tag and schema

    #region Hidden endpoint sharing its route with a visible one

    [Test]
    public async Task SharedRoute_WhenDisplayHiddenEndpointsIsFalse_ShouldOnlyKeepTheVisibleOperationAndNotWarn()
    {
        List<CapturedLog> logs = [];

        using JsonDocument document = await GetDocumentAsync(displayHiddenEndpoints: false, logs: logs);

        JsonElement path = document.RootElement.GetProperty("paths").GetProperty(SharedRoutePath);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(path.TryGetProperty("get", out _), Is.True);
            Assert.That(path.TryGetProperty("post", out _), Is.False);
            Assert.That(logs.Where(w => w.Level >= LogLevel.Warning), Is.Empty, "Removing a hidden endpoint is expected and must not be reported as a discrepancy");
        }
    }

    [Test]
    public async Task SharedRoute_WhenDisplayHiddenEndpointsIsTrue_ShouldKeepBothOperations()
    {
        List<CapturedLog> logs = [];

        using JsonDocument document = await GetDocumentAsync(displayHiddenEndpoints: true, logs: logs);

        JsonElement path = document.RootElement.GetProperty("paths").GetProperty(SharedRoutePath);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(path.TryGetProperty("get", out _), Is.True);
            Assert.That(path.TryGetProperty("post", out _), Is.True);
            Assert.That(logs.Where(w => w.Level >= LogLevel.Warning), Is.Empty);
        }
    }

    /// <summary>
    /// Proves the log capture works, so the "no warning" assertions above cannot pass by accident
    /// </summary>
    [Test]
    public async Task SkippedEndpoint_ShouldBeLogged()
    {
        List<CapturedLog> logs = [];

        using JsonDocument document = await GetDocumentAsync(logs: logs);

        Assert.That(logs.Where(w => w.Level == LogLevel.Debug && w.Message.Contains(MinimalApiPath, StringComparison.Ordinal)), Is.Not.Empty);
    }

    #endregion Hidden endpoint sharing its route with a visible one

    #region Helpers

    private sealed record CapturedLog(LogLevel Level, string Message);

    private sealed class CapturingLoggerProvider(List<CapturedLog> logs) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return categoryName.EndsWith("OpenApiDocumentFilter", StringComparison.Ordinal) ? new CapturingLogger(logs) : Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger(List<CapturedLog> logs) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            logs.Add(new(logLevel, formatter(state, exception)));
        }
    }


    private const string SchemaReferencePrefix = "#/components/schemas/";

    private static void CollectSchemaReferences(JsonElement element, List<string> references)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Name == "$ref" && property.Value.ValueKind == JsonValueKind.String && property.Value.GetString() is { } reference && reference.StartsWith(SchemaReferencePrefix, StringComparison.Ordinal))
                    {
                        references.Add(reference[SchemaReferencePrefix.Length..]);
                    }
                    else
                    {
                        CollectSchemaReferences(property.Value, references);
                    }
                }

                break;
            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    CollectSchemaReferences(item, references);
                }

                break;
        }
    }

    private static JsonElement GetOperation(JsonDocument document, string path)
    {
        return document.RootElement.GetProperty("paths").GetProperty(path).GetProperty("get");
    }

    private async Task<JsonDocument> GetDocumentAsync(string documentName = OpenApiTestControllers.DocumentName, bool displayHiddenEndpoints = false, bool addStaleExtensions = false, List<CapturedLog>? logs = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        if (logs != null)
        {
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            builder.Logging.AddProvider(new CapturingLoggerProvider(logs));
        }

        builder.Services.AddSingleton<IErrorLocalizerService, TestErrorLocalizerService>();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(OpenApiTestController).Assembly)
            .AddApiExceptionHandler<TestResources>(o =>
            {
                o.InternalServerErrorInternalErrorCode = 500000;
                o.UnhandledValidationInternalErrorCode = 412000;
            })
            .AddApiRateLimiting(o =>
            {
                o.RateLimitInternalErrorCode = RateLimitCode;
            })
            .AddOpenApiDocumentation(o =>
            {
                o.DisplayHiddenEndpoints = displayHiddenEndpoints;
                o.OpenApiDocumentInfos =
                [
                    new() { Name = OpenApiTestControllers.DocumentName, Title = "Test document", Version = "1" },
                    new() { Name = OpenApiTestControllers.OtherDocumentName, Title = "Other document", Version = "1" },
                    new() { Name = OpenApiTestControllers.GenericDocumentName, Title = "Generic document", Version = "1" },
                    new() { Name = OpenApiTestControllers.RateLimitDocumentName, Title = "Rate limit document", Version = "1" }
                ];
            });

        if (addStaleExtensions)
        {
            builder.Services.Configure<OpenApiOptions>(OpenApiTestControllers.DocumentName, o =>
            {
                o.AddOperationTransformer((operation, _, _) =>
                {
                    operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                    operation.Extensions["x-access-rights"] = new JsonNodeExtension(new JsonObject { ["privileges"] = "stale", ["privacy"] = "stale" });
                    operation.Extensions["x-ratelimit"] = new JsonNodeExtension(new JsonObject { ["time-in-milliseconds"] = -1 });

                    return Task.CompletedTask;
                });
            });
        }

        WebApplication webApplication = builder.Build();

        _webApplications.Add(webApplication);

        webApplication.UseApiExceptionHandler();
        webApplication.UseOutputCache();
        webApplication.UseOpenApiDocumentation();
        webApplication.MapControllers();

        // Not controller based, so it is flagged as skipped and must never be documented
        webApplication.MapGet(MinimalApiPath, () => "minimal");

        await webApplication.StartAsync();

        using HttpResponseMessage response = await webApplication.GetTestClient().GetAsync($"/openapi/{documentName}.json");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The OpenAPI document could not be generated");

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    #endregion Helpers
}
