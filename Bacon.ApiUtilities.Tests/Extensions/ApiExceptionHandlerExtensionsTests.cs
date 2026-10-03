using Bacon.ApiUtilities.Models.ModelStateValidations;
using Bacon.ApiUtilities.Tests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bacon.ApiUtilities.Tests.Extensions;

[TestFixture]
internal sealed class ApiExceptionHandlerExtensionsTests
{
    #region Options resolution

    [Test]
    public void AddApiExceptionHandler_WithValidCodes_ShouldExposeTheConfiguredCodes()
    {
        using ServiceProvider provider = OptionsTestSetup.BuildProvider(412000, 500000);

        ModelStateValidationOptions options = provider.GetRequiredService<IOptions<ModelStateValidationOptions>>().Value;

        Assert.Multiple(() =>
        {
            Assert.That(options.UnhandledValidationInternalErrorCode, Is.EqualTo(412000));
            Assert.That(options.InternalServerErrorInternalErrorCode, Is.EqualTo(500000));
        });
    }

    [TestCase(0, 500000)]
    [TestCase(-412000, 500000)]
    [TestCase(999999, 500000)]
    [TestCase(412000, 0)]
    [TestCase(412000, 412000)]
    [TestCase(412000, 999999)]
    public void AddApiExceptionHandler_WithInvalidCodes_ShouldThrowOptionsValidationException(long unhandledValidationCode, long internalServerErrorCode)
    {
        using ServiceProvider provider = OptionsTestSetup.BuildProvider(unhandledValidationCode, internalServerErrorCode);

        Assert.That(() => provider.GetRequiredService<IOptions<ModelStateValidationOptions>>().Value, Throws.TypeOf<OptionsValidationException>());
    }

    [Test]
    public void AddApiExceptionHandler_WithInvalidCodes_ShouldReportEveryFailure()
    {
        using ServiceProvider provider = OptionsTestSetup.BuildProvider(0, 0);

        OptionsValidationException? exception = Assert.Throws<OptionsValidationException>(() => _ = provider.GetRequiredService<IOptions<ModelStateValidationOptions>>().Value);

        Assert.That(exception!.Failures, Has.Exactly(2).Items);
    }

    #endregion Options resolution

    #region Host startup

    [Test]
    public async Task AddApiExceptionHandler_WithInvalidCodes_ShouldFailHostStartup()
    {
        WebApplication webApplication = OptionsTestSetup.BuildWebApplication(0, 500000);

        await using (webApplication)
        {
            await Assert.ThatAsync(() => webApplication.StartAsync(), Throws.TypeOf<OptionsValidationException>());
        }
    }

    [Test]
    public async Task AddApiExceptionHandler_WithValidCodes_ShouldStartTheHost()
    {
        WebApplication webApplication = OptionsTestSetup.BuildWebApplication(412000, 500000);

        await using (webApplication)
        {
            await Assert.ThatAsync(() => webApplication.StartAsync(), Throws.Nothing);
            await webApplication.StopAsync();
        }
    }

    #endregion Host startup
}
