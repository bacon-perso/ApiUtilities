using Bacon.ApiUtilities.Tests.Support;
using System.Net;
using System.Text.Json;

namespace Bacon.ApiUtilities.Tests.RateLimiting;

/// <summary>
/// The application configures its own rate limiting before calling AddApiRateLimiting.
/// The library must add to it, not replace it.
/// </summary>
[TestFixture]
internal sealed class ConsumerRateLimitingTests
{
    private RateLimitTestHost _host = null!;

    [SetUp]
    public async Task SetUp()
    {
        _host = await RateLimitTestHost.StartAsync(ConsumerRateLimiting.Add);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _host.DisposeAsync();
    }

    [Test]
    public async Task ConsumerNamedPolicy_Rejection_ShouldUseTheConsumerHandler()
    {
        using HttpResponseMessage first = await _host.GetAsync("/ratelimit/consumer-policy");
        using HttpResponseMessage rejected = await _host.GetAsync("/ratelimit/consumer-policy");

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(rejected.Headers.Contains(ConsumerRateLimiting.HandlerHeaderName), Is.True);
        });
    }

    [Test]
    public async Task ConsumerGlobalLimiter_ShouldStillApply()
    {
        using HttpResponseMessage first = await _host.GetAsync(ConsumerRateLimiting.GlobalLimitedPath);
        using HttpResponseMessage rejected = await _host.GetAsync(ConsumerRateLimiting.GlobalLimitedPath);

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(rejected.Headers.Contains(ConsumerRateLimiting.HandlerHeaderName), Is.True);
        });
    }

    [Test]
    public async Task EndpointRateLimit_WithConsumerLimiterConfigured_ShouldStillReturnErrorResult()
    {
        using HttpResponseMessage first = await _host.GetAsync("/ratelimit/a");
        using HttpResponseMessage rejected = await _host.GetAsync("/ratelimit/a");

        using JsonDocument document = JsonDocument.Parse(await rejected.Content.ReadAsStringAsync());

        Assert.Multiple(() =>
        {
            Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(EndpointRateLimitTests.GetProperty(document.RootElement, "internalCode").GetInt64(), Is.EqualTo(RateLimitTestHost.RateLimitInternalErrorCode));
            Assert.That(rejected.Headers.Contains(ConsumerRateLimiting.HandlerHeaderName), Is.False);
        });
    }

    [Test]
    public async Task ConsumerNamedPolicy_Rejection_ShouldNotBeReportedAsAnEndpointRateLimit()
    {
        using HttpResponseMessage first = await _host.GetAsync("/ratelimit/consumer-policy");
        using HttpResponseMessage rejected = await _host.GetAsync("/ratelimit/consumer-policy");

        string body = await rejected.Content.ReadAsStringAsync();

        Assert.That(body, Does.Not.Contain(RateLimitTestHost.RateLimitInternalErrorCode.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }
}
