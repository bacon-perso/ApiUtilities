using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Bacon.ApiUtilities.Models.Apis.RateLimits;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Bacon.ApiUtilities.Services.Apis;
using Bacon.ApiUtilities.Tests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bacon.ApiUtilities.Tests.Services;

[TestFixture]
internal sealed class EndpointMetadataServiceTests
{
    private const long RootPropertyCode = 1001;
    private const long MiddlePropertyCode = 1002;
    private const long LeafPropertyCode = 1003;
    private const long RootClassCode = 1004;
    private const long MiddleClassCode = 1005;
    private const long LeafClassCode = 1006;
    private const long PathCode = 1007;
    private const long QueryCode = 1008;
    private const long FormPropertyCode = 1009;
    private const long FormItemPropertyCode = 1010;

    private WebApplication _webApplication = null!;

    [SetUp]
    public async Task SetUp()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Logging.ClearProviders();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IErrorLocalizerService, TestErrorLocalizerService>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(NestedValidationTestController).Assembly)
            .AddApiExceptionHandler<TestResources>(o =>
            {
                o.InternalServerErrorInternalErrorCode = 500000;
                o.UnhandledValidationInternalErrorCode = 412000;
            });

        _webApplication = builder.Build();

        _webApplication.MapGet("/minimal/unnamed", () => "x");

        await _webApplication.StartAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _webApplication.StopAsync();
        await _webApplication.DisposeAsync();
    }

    [TestCase(nameof(NestedValidationTestController.Body))]
    [TestCase(nameof(NestedValidationTestController.BodyList))]
    public void GetEndpointInformation_ThreeLevelNestedLists_ShouldCollectValidatorsOfEveryLevel(string actionName)
    {
        EndpointInformation endpointInformation = GetEndpointInformation(actionName);

        Assert.That(
            endpointInformation.ResponseCodes,
            Is.SupersetOf(new[] { RootPropertyCode, MiddlePropertyCode, LeafPropertyCode, RootClassCode, MiddleClassCode, LeafClassCode }));
    }

    [Test]
    public void GetEndpointInformation_PathQueryAndBody_ShouldCollectValidatorsOfEverySource()
    {
        EndpointInformation endpointInformation = GetEndpointInformation(nameof(NestedValidationTestController.Mixed));

        Assert.That(
            endpointInformation.ResponseCodes,
            Is.SupersetOf(new[] { PathCode, QueryCode, RootPropertyCode, MiddlePropertyCode, LeafPropertyCode, RootClassCode, MiddleClassCode, LeafClassCode }));
    }

    [Test]
    public void GetEndpointInformation_Form_ShouldCollectValidatorsOfNestedList()
    {
        EndpointInformation endpointInformation = GetEndpointInformation(nameof(NestedValidationTestController.Form));

        Assert.That(endpointInformation.ResponseCodes, Is.SupersetOf(new[] { FormPropertyCode, FormItemPropertyCode }));
    }

    [Test]
    public void GetEndpointInformation_ControllerAction_ShouldUseActionNameAsOperationId()
    {
        EndpointInformation endpointInformation = GetEndpointInformation(nameof(NestedValidationTestController.Body));

        Assert.That(endpointInformation.OperationId, Is.EqualTo(nameof(NestedValidationTestController.Body)));
    }

    [Test]
    public void GetEndpointInformation_MinimalApi_ShouldBeSkippedWithVerbAndRoute()
    {
        ApiDescription apiDescription = GetApiDescriptions().Single(s => s.RelativePath == "minimal/unnamed");

        EndpointInformation endpointInformation = GetEndpointInformation(apiDescription);

        Assert.Multiple(() =>
        {
            Assert.That(endpointInformation.IsSkipped, Is.True);
            Assert.That(endpointInformation.Verb, Is.EqualTo("GET"));
            Assert.That(endpointInformation.Route, Is.EqualTo("/minimal/unnamed"));
        });
    }

    [Test]
    public void GetEndpointInformation_ControllerAction_ShouldNotBeSkippedAndShouldUseEndpointTagOverride()
    {
        EndpointInformation endpointInformation = GetEndpointInformation(GetApiDescriptions().Single(s => s.ActionDescriptor.RouteValues["controller"] == "TaggedTest"));

        Assert.Multiple(() =>
        {
            Assert.That(endpointInformation.IsSkipped, Is.False);
            Assert.That(endpointInformation.Metadata.Tag, Is.EqualTo("CustomTag"));
        });
    }

    [Test]
    public void GetEndpointInformation_BodyAndResponseSharingTypes_ShouldKeepEverySchemaAndOnlyBodyValidators()
    {
        EndpointInformation endpointInformation = GetEndpointInformation(nameof(NestedValidationTestController.Shared));

        Assert.Multiple(() =>
        {
            Assert.That(endpointInformation.Schemas, Is.SupersetOf(new[] { typeof(NestedRoot), typeof(NestedMiddle), typeof(NestedLeaf) }));
            Assert.That(endpointInformation.ResponseCodes, Is.SupersetOf(new[] { MiddlePropertyCode, LeafPropertyCode, MiddleClassCode, LeafClassCode }));
            Assert.That(endpointInformation.ResponseCodes, Does.Not.Contain(RootPropertyCode).And.Not.Contain(RootClassCode));
        });
    }

    [Test]
    public void GetEndpointInformation_ThreeLevelNestedLists_ShouldAddEverySchema()
    {
        EndpointInformation endpointInformation = GetEndpointInformation(nameof(NestedValidationTestController.Body));

        Assert.That(endpointInformation.Schemas, Is.SupersetOf(new[] { typeof(NestedRoot), typeof(NestedMiddle), typeof(NestedLeaf) }));
    }

    private IEnumerable<ApiDescription> GetApiDescriptions()
    {
        return _webApplication.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups.Items
            .SelectMany(s => s.Items);
    }

    private EndpointInformation GetEndpointInformation(string actionName)
    {
        ApiDescription apiDescription = GetApiDescriptions()
            .Single(s => s.ActionDescriptor.RouteValues["controller"] == "NestedValidationTest" && s.ActionDescriptor.RouteValues["action"] == actionName);

        return GetEndpointInformation(apiDescription);
    }

    private EndpointInformation GetEndpointInformation(ApiDescription apiDescription)
    {
        IServiceProvider services = _webApplication.Services;

        EndpointMetadataService endpointMetadataService = new(services.GetRequiredService<IModelMetadataProvider>());

        ModelStateValidationOptions options = new()
        {
            UnhandledValidationInternalErrorCode = 412000,
            InternalServerErrorInternalErrorCode = 500000,
            CustomValidationAttributeSettings = new Dictionary<Type, IEnumerable<long>>
            {
                { typeof(RootPropertyMarkerAttribute), [RootPropertyCode] },
                { typeof(MiddlePropertyMarkerAttribute), [MiddlePropertyCode] },
                { typeof(LeafPropertyMarkerAttribute), [LeafPropertyCode] },
                { typeof(RootClassMarkerAttribute), [RootClassCode] },
                { typeof(MiddleClassMarkerAttribute), [MiddleClassCode] },
                { typeof(LeafClassMarkerAttribute), [LeafClassCode] },
                { typeof(PathMarkerAttribute), [PathCode] },
                { typeof(QueryMarkerAttribute), [QueryCode] },
                { typeof(FormPropertyMarkerAttribute), [FormPropertyCode] },
                { typeof(FormItemPropertyMarkerAttribute), [FormItemPropertyCode] }
            }
        };

        return endpointMetadataService.GetEndpointInformation(apiDescription, options, new RateLimitOptions { RateLimitInternalErrorCode = 429000 });
    }
}
