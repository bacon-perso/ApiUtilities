using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Bacon.ApiUtilities.Services.ModelStateValidations;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bacon.ApiUtilities.Tests.Services;

[TestFixture]
internal sealed class ModelValidationServiceTests
{
    private const string MaxLengthTemplate = "{0} must have at most {1} characters";
    private const string MaxLengthResourceKey = "400123";

    private sealed class SampleModel
    {
        [JsonPropertyName("full_name")]
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }

    /// <summary>
    /// Returns the template registered for a resource key, or the key itself when there is none
    /// </summary>
    private sealed class StubErrorLocalizerService(Dictionary<string, string> resources) : IErrorLocalizerService
    {
        public string GetResourceValue(string resourceKey)
        {
            return resources.TryGetValue(resourceKey, out string? value) ? value : resourceKey;
        }
    }

    private static ModelValidationService CreateService(Dictionary<ValidationAttributeTypes, long>? builtInSettings = null)
    {
        ModelStateValidationOptions options = new()
        {
            BuiltInValidationAttributeSettings = builtInSettings
        };

        Dictionary<string, string> resources = new()
        {
            [MaxLengthResourceKey] = MaxLengthTemplate
        };

        return new(new StubErrorLocalizerService(resources), Options.Create(options));
    }

    private static ValidationContext CreateContext(string displayName)
    {
        return new(new SampleModel()) { DisplayName = displayName };
    }

    private static ErrorResult Parse(ValidationResult validationResult)
    {
        // Same call as ModelStateApiBehaviorOptionsService uses to read the message back
        return JsonSerializer.Deserialize<ErrorResult>(validationResult.ErrorMessage!, JsonSerializerOptions.Web)!;
    }

    #region Built-in attribute overload

    [Test]
    public void BuiltIn_WithoutSettings_ShouldFallBackToPreconditionFailed()
    {
        ModelValidationService service = CreateService();

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Age)), ValidationAttributeTypes.AllowedMaxLengthAttribute));

        Assert.Multiple(() =>
        {
            Assert.That(errorResult.InternalCode, Is.EqualTo(412));
            Assert.That(errorResult.HttpStatusCode, Is.EqualTo((int)HttpStatusCode.PreconditionFailed));
        });
    }

    [Test]
    public void BuiltIn_WithSettingsForAnotherAttribute_ShouldFallBackToPreconditionFailed()
    {
        ModelValidationService service = CreateService(new() { [ValidationAttributeTypes.EmailFormatAttribute] = 400999 });

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Age)), ValidationAttributeTypes.AllowedMaxLengthAttribute));

        Assert.Multiple(() =>
        {
            Assert.That(errorResult.InternalCode, Is.EqualTo(412));
            Assert.That(errorResult.HttpStatusCode, Is.EqualTo((int)HttpStatusCode.PreconditionFailed));
        });
    }

    [TestCase(400123, HttpStatusCode.BadRequest)]
    [TestCase(404001, HttpStatusCode.NotFound)]
    [TestCase(4220, HttpStatusCode.UnprocessableEntity)]
    public void BuiltIn_WithConfiguredCode_ShouldUseItAndItsHttpStatusCode(long internalErrorCode, HttpStatusCode expectedHttpStatusCode)
    {
        ModelValidationService service = CreateService(new() { [ValidationAttributeTypes.AllowedMaxLengthAttribute] = internalErrorCode });

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Age)), ValidationAttributeTypes.AllowedMaxLengthAttribute));

        Assert.Multiple(() =>
        {
            Assert.That(errorResult.InternalCode, Is.EqualTo(internalErrorCode));
            Assert.That(errorResult.HttpStatusCode, Is.EqualTo((int)expectedHttpStatusCode));
        });
    }

    [TestCase(12L)]
    [TestCase(600000L)]
    public void BuiltIn_WithConfiguredCodeThatHasNoHttpStatusCode_ShouldKeepTheCodeAndUsePreconditionFailed(long internalErrorCode)
    {
        ModelValidationService service = CreateService(new() { [ValidationAttributeTypes.AllowedMaxLengthAttribute] = internalErrorCode });

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Age)), ValidationAttributeTypes.AllowedMaxLengthAttribute));

        Assert.Multiple(() =>
        {
            Assert.That(errorResult.InternalCode, Is.EqualTo(internalErrorCode));
            Assert.That(errorResult.HttpStatusCode, Is.EqualTo((int)HttpStatusCode.PreconditionFailed));
        });
    }

    [Test]
    public void BuiltIn_WithConfiguredCode_ShouldFormatTheLocalizedMessageWithTheFieldNameAndData()
    {
        ModelValidationService service = CreateService(new() { [ValidationAttributeTypes.AllowedMaxLengthAttribute] = long.Parse(MaxLengthResourceKey) });

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Age)), ValidationAttributeTypes.AllowedMaxLengthAttribute, 10));

        Assert.Multiple(() =>
        {
            Assert.That(errorResult.Message, Is.EqualTo("Age must have at most 10 characters"));
            Assert.That(errorResult.Metadata!.Select(s => s.ToString()), Is.EqualTo(["Age", "10"]));
        });
    }

    #endregion Built-in attribute overload

    #region Field name

    [Test]
    public void FieldName_WithJsonPropertyNameAttribute_ShouldUseTheJsonName()
    {
        ModelValidationService service = CreateService(new() { [ValidationAttributeTypes.AllowedMaxLengthAttribute] = long.Parse(MaxLengthResourceKey) });

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Name)), ValidationAttributeTypes.AllowedMaxLengthAttribute, 5));

        Assert.Multiple(() =>
        {
            Assert.That(errorResult.Message, Is.EqualTo("full_name must have at most 5 characters"));
            Assert.That(errorResult.Metadata!.First().ToString(), Is.EqualTo("full_name"));
        });
    }

    [Test]
    public void FieldName_WithoutJsonPropertyNameAttribute_ShouldUseThePropertyName()
    {
        ModelValidationService service = CreateService(new() { [ValidationAttributeTypes.AllowedMaxLengthAttribute] = long.Parse(MaxLengthResourceKey) });

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Age)), ValidationAttributeTypes.AllowedMaxLengthAttribute, 5));

        Assert.That(errorResult.Metadata!.First().ToString(), Is.EqualTo("Age"));
    }

    [Test]
    public void FieldName_ThatIsNotAProperty_ShouldUseTheDisplayNameAsParameterName()
    {
        ModelValidationService service = CreateService(new() { [ValidationAttributeTypes.AllowedMaxLengthAttribute] = long.Parse(MaxLengthResourceKey) });

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext("someQueryParameter"), ValidationAttributeTypes.AllowedMaxLengthAttribute, 5));

        Assert.That(errorResult.Metadata!.First().ToString(), Is.EqualTo("someQueryParameter"));
    }

    #endregion Field name

    #region Explicit overload

    [Test]
    public void Explicit_ShouldUseTheGivenValuesAsIs()
    {
        ModelValidationService service = CreateService();

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Age)), MaxLengthResourceKey, HttpStatusCode.Conflict, 409555, 3, "x"));

        Assert.Multiple(() =>
        {
            Assert.That(errorResult.InternalCode, Is.EqualTo(409555));
            Assert.That(errorResult.HttpStatusCode, Is.EqualTo((int)HttpStatusCode.Conflict));
            Assert.That(errorResult.Message, Is.EqualTo("Age must have at most 3 characters"));
            Assert.That(errorResult.Metadata!.Select(s => s.ToString()), Is.EqualTo(["Age", "3", "x"]));
        });
    }

    [Test]
    public void Explicit_WithoutMessageData_ShouldStillContainTheFieldNameAsMetadata()
    {
        ModelValidationService service = CreateService();

        ErrorResult errorResult = Parse(service.CreateInvalidModelValidationResult(CreateContext(nameof(SampleModel.Age)), "no-such-key", HttpStatusCode.BadRequest, 400001));

        Assert.Multiple(() =>
        {
            Assert.That(errorResult.Message, Is.EqualTo("no-such-key"));
            Assert.That(errorResult.Metadata!.Select(s => s.ToString()), Is.EqualTo(["Age"]));
        });
    }

    #endregion Explicit overload
}
