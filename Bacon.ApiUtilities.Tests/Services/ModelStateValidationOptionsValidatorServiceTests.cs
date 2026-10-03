using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Bacon.ApiUtilities.Services.Apis.Validators;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Bacon.ApiUtilities.Tests.Services;

[TestFixture]
internal sealed class ModelStateValidationOptionsValidatorServiceTests
{
    private readonly ModelStateValidationOptionsValidatorService _validator = new();

    #region Unhandled validation code

    [Test]
    public void Validate_WithValidCodes_ShouldSucceed()
    {
        ValidateOptionsResult result = Validate(CreateValidOptions());

        Assert.That(result.Succeeded, Is.True);
    }

    [TestCase(0)]
    [TestCase(-412000)]
    [TestCase(99)]
    [TestCase(999999)]
    public void Validate_WithInvalidUnhandledValidationCode_ShouldFail(long code)
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.UnhandledValidationInternalErrorCode = code;

        ValidateOptionsResult result = Validate(options);

        Assert.That(result.Failed, Is.True);
    }

    #endregion Unhandled validation code

    #region Internal server error code

    [TestCase(0)]
    [TestCase(-500000)]
    [TestCase(412000)]
    [TestCase(999999)]
    public void Validate_WithInvalidInternalServerErrorCode_ShouldFail(long code)
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.InternalServerErrorInternalErrorCode = code;

        ValidateOptionsResult result = Validate(options);

        Assert.That(result.Failed, Is.True);
    }

    [Test]
    public void Validate_WithNonServerErrorCode_ShouldMentionThat500IsExpected()
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.InternalServerErrorInternalErrorCode = 412000;

        ValidateOptionsResult result = Validate(options);

        Assert.That(string.Join(";", result.Failures!), Does.Contain("500"));
    }

    #endregion Internal server error code

    #region Built-in validation attributes

    [Test]
    public void Validate_WithValidBuiltInAttributeCode_ShouldSucceed()
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.BuiltInValidationAttributeSettings = new Dictionary<ValidationAttributeTypes, long>
        {
            { ValidationAttributeTypes.EmailFormatAttribute, 412001 }
        };

        ValidateOptionsResult result = Validate(options);

        Assert.That(result.Succeeded, Is.True);
    }

    [TestCase(0)]
    [TestCase(-412001)]
    [TestCase(999999)]
    public void Validate_WithInvalidBuiltInAttributeCode_ShouldFailAndNameTheAttribute(long code)
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.BuiltInValidationAttributeSettings = new Dictionary<ValidationAttributeTypes, long>
        {
            { ValidationAttributeTypes.EmailFormatAttribute, code }
        };

        ValidateOptionsResult result = Validate(options);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(string.Join(";", result.Failures!), Does.Contain(nameof(ValidationAttributeTypes.EmailFormatAttribute)));
        });
    }

    #endregion Built-in validation attributes

    #region Custom validation attributes

    [Test]
    public void Validate_WithValidCustomAttribute_ShouldSucceed()
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.CustomValidationAttributeSettings = new Dictionary<Type, IEnumerable<long>>
        {
            { typeof(RequiredAttribute), [412002, 412003] }
        };

        ValidateOptionsResult result = Validate(options);

        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void Validate_WithCustomTypeThatIsNotAValidationAttribute_ShouldFail()
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.CustomValidationAttributeSettings = new Dictionary<Type, IEnumerable<long>>
        {
            { typeof(string), [412002] }
        };

        ValidateOptionsResult result = Validate(options);

        Assert.Multiple(() =>
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(string.Join(";", result.Failures!), Does.Contain(nameof(String)));
        });
    }

    [TestCase(0)]
    [TestCase(-412002)]
    [TestCase(999999)]
    public void Validate_WithInvalidCustomAttributeCode_ShouldFail(long code)
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.CustomValidationAttributeSettings = new Dictionary<Type, IEnumerable<long>>
        {
            { typeof(RequiredAttribute), [code] }
        };

        ValidateOptionsResult result = Validate(options);

        Assert.That(result.Failed, Is.True);
    }

    #endregion Custom validation attributes

    #region Aggregation

    [Test]
    public void Validate_WithSeveralProblems_ShouldReportEachOne()
    {
        ModelStateValidationOptions options = new()
        {
            UnhandledValidationInternalErrorCode = 0,
            InternalServerErrorInternalErrorCode = 0
        };

        ValidateOptionsResult result = Validate(options);

        Assert.That(result.Failures, Has.Exactly(2).Items);
    }

    #endregion Aggregation

    #region Code length boundaries

    [TestCase(412, 500)]
    [TestCase(4120, 5000)]
    [TestCase(412, 500999)]
    public void Validate_WithShortButWellFormedCodes_ShouldSucceed(long unhandledValidationCode, long internalServerErrorCode)
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.UnhandledValidationInternalErrorCode = unhandledValidationCode;
        options.InternalServerErrorInternalErrorCode = internalServerErrorCode;

        ValidateOptionsResult result = Validate(options);

        Assert.That(result.Succeeded, Is.True);
    }

    [TestCase(41, 500000)]
    [TestCase(412000, 50)]
    public void Validate_WithTwoDigitCodes_ShouldFail(long unhandledValidationCode, long internalServerErrorCode)
    {
        ModelStateValidationOptions options = CreateValidOptions();
        options.UnhandledValidationInternalErrorCode = unhandledValidationCode;
        options.InternalServerErrorInternalErrorCode = internalServerErrorCode;

        ValidateOptionsResult result = Validate(options);

        Assert.That(result.Failed, Is.True);
    }

    #endregion Code length boundaries

    #region Helpers

    private static ModelStateValidationOptions CreateValidOptions()
    {
        return new()
        {
            UnhandledValidationInternalErrorCode = 412000,
            InternalServerErrorInternalErrorCode = 500000
        };
    }

    private ValidateOptionsResult Validate(ModelStateValidationOptions options)
    {
        return _validator.Validate(Options.DefaultName, options);
    }

    #endregion Helpers
}
