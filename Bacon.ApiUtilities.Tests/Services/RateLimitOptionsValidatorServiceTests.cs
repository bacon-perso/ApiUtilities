using Bacon.ApiUtilities.Models.Apis.RateLimits;
using Bacon.ApiUtilities.Services.Apis.Validators;
using Microsoft.Extensions.Options;

namespace Bacon.ApiUtilities.Tests.Services;

[TestFixture]
internal sealed class RateLimitOptionsValidatorServiceTests
{
    private readonly RateLimitOptionsValidatorService _validator = new();

    [TestCase(429000)]
    [TestCase(429001)]
    [TestCase(429999)]
    [TestCase(429)]
    [TestCase(4290)]
    [TestCase(42900)]
    public void Validate_WithValidCode_ShouldSucceed(long rateLimitCode)
    {
        ValidateOptionsResult result = Validate(rateLimitCode);

        Assert.That(result.Succeeded, Is.True);
    }

    [TestCase(0)]
    [TestCase(-429000)]
    [TestCase(500000)]
    [TestCase(404001)]
    [TestCase(999999)]
    [TestCase(42)]
    [TestCase(428999)]
    public void Validate_WithInvalidCode_ShouldFail(long rateLimitCode)
    {
        ValidateOptionsResult result = Validate(rateLimitCode);

        Assert.That(result.Failed, Is.True);
    }

    [TestCase(0, "cannot be 0 or less")]
    [TestCase(-429000, "cannot be 0 or less")]
    [TestCase(999999, "format")]
    [TestCase(500000, "429")]
    [TestCase(404001, "429")]
    [TestCase(42, "format")]
    [TestCase(428999, "429")]
    public void Validate_WithInvalidCode_ShouldExplainWhy(long rateLimitCode, string expectedFragment)
    {
        ValidateOptionsResult result = Validate(rateLimitCode);

        Assert.That(string.Join(";", result.Failures!), Does.Contain(expectedFragment));
    }

    private ValidateOptionsResult Validate(long rateLimitCode)
    {
        RateLimitOptions options = new()
        {
            RateLimitInternalErrorCode = rateLimitCode
        };

        return _validator.Validate(Options.DefaultName, options);
    }
}
