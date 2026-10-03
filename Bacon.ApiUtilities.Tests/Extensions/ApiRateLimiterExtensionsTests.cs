using Bacon.ApiUtilities.Models.Apis.RateLimits;
using Bacon.ApiUtilities.Tests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bacon.ApiUtilities.Tests.Extensions;

[TestFixture]
internal sealed class ApiRateLimiterExtensionsTests
{
    private const long ValidUnhandledValidationCode = 412000;

    private const long ValidInternalServerErrorCode = 500000;

    #region Options resolution

    [TestCase(429000)]
    [TestCase(429001)]
    [TestCase(429999)]
    public void AddApiRateLimiting_WithValidRateLimitCode_ShouldExposeTheConfiguredCode(long rateLimitCode)
    {
        using ServiceProvider provider = OptionsTestSetup.BuildProvider(ValidUnhandledValidationCode, ValidInternalServerErrorCode, rateLimitCode);

        RateLimitOptions options = provider.GetRequiredService<IOptions<RateLimitOptions>>().Value;

        Assert.That(options.RateLimitInternalErrorCode, Is.EqualTo(rateLimitCode));
    }

    [TestCase(0)]
    [TestCase(-429000)]
    [TestCase(500000)]
    [TestCase(404001)]
    [TestCase(999999)]
    public void AddApiRateLimiting_WithInvalidRateLimitCode_ShouldThrowOptionsValidationException(long rateLimitCode)
    {
        using ServiceProvider provider = OptionsTestSetup.BuildProvider(ValidUnhandledValidationCode, ValidInternalServerErrorCode, rateLimitCode);

        Assert.That(() => provider.GetRequiredService<IOptions<RateLimitOptions>>().Value, Throws.TypeOf<OptionsValidationException>());
    }

    [Test]
    public void AddApiRateLimiting_WithNonRateLimitHttpStatusCode_ShouldMentionExpectedPrefix()
    {
        using ServiceProvider provider = OptionsTestSetup.BuildProvider(ValidUnhandledValidationCode, ValidInternalServerErrorCode, 500000);

        OptionsValidationException? exception = Assert.Throws<OptionsValidationException>(() => _ = provider.GetRequiredService<IOptions<RateLimitOptions>>().Value);

        Assert.That(exception!.Message, Does.Contain("429"));
    }

    #endregion Options resolution

    #region Host startup

    [Test]
    public async Task AddApiRateLimiting_WithInvalidRateLimitCode_ShouldFailHostStartup()
    {
        WebApplication webApplication = OptionsTestSetup.BuildWebApplication(ValidUnhandledValidationCode, ValidInternalServerErrorCode, 500000);

        await using (webApplication)
        {
            await Assert.ThatAsync(() => webApplication.StartAsync(), Throws.TypeOf<OptionsValidationException>());
        }
    }

    [Test]
    public async Task AddApiRateLimiting_WithValidRateLimitCode_ShouldStartTheHost()
    {
        WebApplication webApplication = OptionsTestSetup.BuildWebApplication(ValidUnhandledValidationCode, ValidInternalServerErrorCode, 429001);

        await using (webApplication)
        {
            await Assert.ThatAsync(() => webApplication.StartAsync(), Throws.Nothing);
            await webApplication.StopAsync();
        }
    }

    #endregion Host startup
}
