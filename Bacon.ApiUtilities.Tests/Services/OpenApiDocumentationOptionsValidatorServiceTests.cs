using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Bacon.ApiUtilities.Services.Apis.Validators;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Bacon.ApiUtilities.Tests.Services;

[TestFixture]
internal sealed class OpenApiDocumentationOptionsValidatorServiceTests
{
    private readonly OpenApiDocumentationOptionsValidatorService _validator = new();

    private static OpenApiDocumentationOptions CreateOptions(Action<OpenApiDocumentationOptions>? configure = null)
    {
        OpenApiDocumentationOptions options = new()
        {
            OpenApiDocumentInfos = [new() { Name = "doc1", Title = "Document 1", Version = "1" }]
        };

        configure?.Invoke(options);

        return options;
    }

    private ValidateOptionsResult Validate(Action<OpenApiDocumentationOptions>? configure = null)
    {
        return _validator.Validate(null, CreateOptions(configure));
    }

    private static string Failures(ValidateOptionsResult result)
    {
        return string.Join(";", result.Failures ?? []);
    }

    #region Defaults

    [Test]
    public void Validate_WithDefaults_ShouldSucceed()
    {
        Assert.That(Validate().Succeeded, Is.True);
    }

    #endregion Defaults

    #region Document infos

    [Test]
    public void Validate_WithoutDocuments_ShouldFail()
    {
        ValidateOptionsResult result = Validate(o => o.OpenApiDocumentInfos = []);

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        ValidateOptionsResult result = Validate(o => o.OpenApiDocumentInfos = [new() { Name = "doc1", Title = " ", Version = "1" }]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("title"));
        });
    }

    [Test]
    public void Validate_WithEmptyName_ShouldFail()
    {
        ValidateOptionsResult result = Validate(o => o.OpenApiDocumentInfos = [new() { Name = "", Title = "Document 1", Version = "1" }]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("empty name"));
        });
    }

    [Test]
    public void Validate_WithEmptyVersion_ShouldFail()
    {
        ValidateOptionsResult result = Validate(o => o.OpenApiDocumentInfos = [new() { Name = "doc1", Title = "Document 1", Version = "" }]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("version"));
        });
    }

    [Test]
    public void Validate_WithDuplicatedNames_ShouldFailAndListTheDuplicates()
    {
        ValidateOptionsResult result = Validate(o => o.OpenApiDocumentInfos =
        [
            new() { Name = "doc1", Title = "Document 1", Version = "1" },
            new() { Name = "doc1", Title = "Document 1 again", Version = "1" }
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("duplicated").And.Contain("doc1"));
        });
    }

    [Test]
    public void Validate_WithWhitespaceInName_ShouldFailAndListTheName()
    {
        ValidateOptionsResult result = Validate(o => o.OpenApiDocumentInfos = [new() { Name = "my doc", Title = "Document 1", Version = "1" }]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("my doc"));
        });
    }

    #endregion Document infos

    #region Output cache duration

    [Test]
    public void Validate_WithNegativeOutputCacheDuration_ShouldFail()
    {
        ValidateOptionsResult result = Validate(o => o.OutputCacheDuration = TimeSpan.FromSeconds(-1));

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("cache duration"));
        });
    }

    [TestCase(0)]
    [TestCase(30)]
    public void Validate_WithZeroOrPositiveOutputCacheDuration_ShouldSucceed(int seconds)
    {
        Assert.That(Validate(o => o.OutputCacheDuration = TimeSpan.FromSeconds(seconds)).Succeeded, Is.True);
    }

    #endregion Output cache duration

    #region UI configs

    [Test]
    public void Validate_WithNullUiConfigs_ShouldFailWithoutThrowing()
    {
        ValidateOptionsResult result = default!;

        Assert.DoesNotThrow(() => result = Validate(o => o.UiConfigs = null!));

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("UI configs"));
        });
    }

    [TestCase(UiTypes.Swagger)]
    [TestCase(UiTypes.Scalar)]
    public void Validate_WithDefinedUiType_ShouldSucceed(UiTypes uiType)
    {
        Assert.That(Validate(o => o.UiConfigs.UiType = uiType).Succeeded, Is.True);
    }

    /// <summary>
    /// A value that is not a member can still get in through a cast, configuration binding or a stored number of a removed member
    /// </summary>
    [TestCase(-1)]
    [TestCase(2)]
    [TestCase(99)]
    public void Validate_WithUndefinedUiType_ShouldFailAndListTheSupportedValues(int uiType)
    {
        ValidateOptionsResult result = Validate(o => o.UiConfigs.UiType = (UiTypes)uiType);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("UI type").And.Contain(uiType.ToString()).And.Contain("Swagger").And.Contain("Scalar"));
        });
    }

    [TestCase("")]
    [TestCase("docs")]
    [TestCase("/docs")]
    [TestCase("docs/")]
    [TestCase("/docs/")]
    [TestCase("api/docs")]
    [TestCase("/api/docs/")]
    [TestCase("api-docs_v1.0~x")]
    public void Validate_WithValidVirtualPath_ShouldSucceed(string virtualPath)
    {
        Assert.That(Validate(o => o.UiConfigs.VirtualPath = virtualPath).Succeeded, Is.True);
    }

    [Test]
    public void Validate_WithNullVirtualPath_ShouldSucceed()
    {
        Assert.That(Validate(o => o.UiConfigs.VirtualPath = null!).Succeeded, Is.True);
    }

    [TestCase("my docs")]
    [TestCase("docs?x=1")]
    [TestCase("docs#top")]
    [TestCase("a//b")]
    [TestCase("../x")]
    [TestCase("./x")]
    [TestCase("a/../b")]
    [TestCase("{id}")]
    [TestCase(@"a\b")]
    [TestCase("docs%20x")]
    public void Validate_WithInvalidVirtualPath_ShouldFail(string virtualPath)
    {
        ValidateOptionsResult result = Validate(o => o.UiConfigs.VirtualPath = virtualPath);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("virtual path"));
        });
    }

    [Test]
    public void Validate_WithDuplicatedSubmitMethods_ShouldFailAndListThem()
    {
        ValidateOptionsResult result = Validate(o => o.UiConfigs.SupportedSubmitMethods = [SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Get]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("duplicated submit methods").And.Contain("Get"));
        });
    }

    [Test]
    public void Validate_WithDistinctOrEmptySubmitMethods_ShouldSucceed()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Validate(o => o.UiConfigs.SupportedSubmitMethods = [SubmitMethod.Get, SubmitMethod.Post]).Succeeded, Is.True);
            Assert.That(Validate(o => o.UiConfigs.SupportedSubmitMethods = []).Succeeded, Is.True);
            Assert.That(Validate(o => o.UiConfigs.SupportedSubmitMethods = null).Succeeded, Is.True);
        });
    }

    [Test]
    public void Validate_WithEmptyCorsPolicyName_ShouldFail()
    {
        ValidateOptionsResult result = Validate(o => o.UiConfigs.CorsPolicyCss = new() { { " ", "#FFAC1C" } });

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("cors policy names"));
        });
    }

    [TestCase("#FFAC1C")]
    [TestCase("#fff")]
    [TestCase("#4285f4")]
    public void Validate_WithValidHexColor_ShouldSucceed(string color)
    {
        Assert.That(Validate(o => o.UiConfigs.CorsPolicyCss = new() { { "Internal", color } }).Succeeded, Is.True);
    }

    [TestCase("red")]
    [TestCase("FFAC1C")]
    [TestCase("#FFAC1")]
    [TestCase("#GGGGGG")]
    [TestCase("#FFAC1C;")]
    [TestCase("")]
    public void Validate_WithInvalidHexColor_ShouldFailAndListTheColor(string color)
    {
        ValidateOptionsResult result = Validate(o => o.UiConfigs.CorsPolicyCss = new() { { "Internal", color } });

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(Failures(result), Does.Contain("Hex colors"));
        });
    }

    [Test]
    public void Validate_WithNullOrEmptyCorsPolicyCss_ShouldSucceed()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Validate(o => o.UiConfigs.CorsPolicyCss = null).Succeeded, Is.True);
            Assert.That(Validate(o => o.UiConfigs.CorsPolicyCss = []).Succeeded, Is.True);
        });
    }

    [Test]
    public void Validate_WithUndefinedUiTypeAndOtherInvalidUiConfigs_ShouldReportAllOfThem()
    {
        ValidateOptionsResult result = Validate(o =>
        {
            o.UiConfigs.UiType = (UiTypes)99;
            o.UiConfigs.VirtualPath = "my docs";
            o.UiConfigs.CorsPolicyCss = new() { { "", "red" } };
        });

        // The UI type, the virtual path, the empty cors policy name and the invalid hex color
        Assert.That(result.Failures!.Count(), Is.EqualTo(4));
    }

    [Test]
    public void Validate_WithSeveralInvalidUiConfigs_ShouldReportAllOfThem()
    {
        ValidateOptionsResult result = Validate(o =>
        {
            o.UiConfigs.VirtualPath = "my docs";
            o.UiConfigs.SupportedSubmitMethods = [SubmitMethod.Get, SubmitMethod.Get];
            o.UiConfigs.CorsPolicyCss = new() { { "", "red" } };
        });

        Assert.That(result.Failures!.Count(), Is.EqualTo(4));
    }

    #endregion UI configs
}
