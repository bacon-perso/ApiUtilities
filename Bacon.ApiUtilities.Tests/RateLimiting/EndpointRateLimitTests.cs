using Bacon.ApiUtilities.Attributes.Apis;
using Bacon.ApiUtilities.Tests.Support;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace Bacon.ApiUtilities.Tests.RateLimiting;

[TestFixture]
internal sealed class EndpointRateLimitTests
{
    private RateLimitTestHost _host = null!;

    [SetUp]
    public async Task SetUp()
    {
        _host = await RateLimitTestHost.StartAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _host.DisposeAsync();
    }

    #region Enforcement

    [Test]
    public async Task RateLimit_FirstCall_ShouldSucceed()
    {
        using HttpResponseMessage response = await _host.GetAsync("/ratelimit/a");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task RateLimit_SecondImmediateCall_ShouldReturnTooManyRequests()
    {
        using HttpResponseMessage first = await _host.GetAsync("/ratelimit/a");
        using HttpResponseMessage second = await _host.GetAsync("/ratelimit/a");

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        });
    }

    [Test]
    public async Task RateLimit_EndpointWithoutAttribute_ShouldNeverBeLimited()
    {
        for (int i = 0; i < 10; i++)
        {
            using HttpResponseMessage response = await _host.GetAsync("/ratelimit/free");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Call {i + 1}");
        }
    }

    [TestCase("/ratelimit/zero")]
    [TestCase("/ratelimit/unset")]
    public async Task RateLimit_EndpointWithZeroMilliSeconds_ShouldNeverBeLimited(string path)
    {
        for (int i = 0; i < 10; i++)
        {
            using HttpResponseMessage response = await _host.GetAsync(path);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Call {i + 1}");
        }
    }

    [Test]
    public async Task RateLimit_AfterTheDelay_ShouldSucceedAgain()
    {
        using HttpResponseMessage first = await _host.GetAsync("/ratelimit/short");
        using HttpResponseMessage second = await _host.GetAsync("/ratelimit/short");

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        });

        // The bucket refills on a timer tick, so poll instead of sleeping for an exact time
        HttpStatusCode lastStatusCode = HttpStatusCode.TooManyRequests;
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);

        while (lastStatusCode == HttpStatusCode.TooManyRequests && DateTime.UtcNow < deadline)
        {
            await Task.Delay(100);

            using HttpResponseMessage retry = await _host.GetAsync("/ratelimit/short");
            lastStatusCode = retry.StatusCode;
        }

        Assert.That(lastStatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    #endregion Enforcement

    #region Partitions

    [Test]
    public async Task RateLimit_DifferentActions_ShouldHaveIndependentLimits()
    {
        using HttpResponseMessage firstOnA = await _host.GetAsync("/ratelimit/a");
        using HttpResponseMessage secondOnA = await _host.GetAsync("/ratelimit/a");
        using HttpResponseMessage firstOnB = await _host.GetAsync("/ratelimit/b");

        Assert.Multiple(() =>
        {
            Assert.That(firstOnA.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(secondOnA.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(firstOnB.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    [Test]
    public async Task RateLimit_OverloadedActions_ShouldHaveIndependentLimits()
    {
        using HttpResponseMessage firstOnOverload = await _host.GetAsync("/ratelimit/overload");
        using HttpResponseMessage secondOnOverload = await _host.GetAsync("/ratelimit/overload");
        using HttpResponseMessage firstOnOtherOverload = await _host.GetAsync("/ratelimit/overload/1");

        Assert.Multiple(() =>
        {
            Assert.That(firstOnOverload.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(secondOnOverload.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(firstOnOtherOverload.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    [Test]
    public async Task RateLimit_DifferentCallers_ShouldHaveIndependentLimits()
    {
        using HttpResponseMessage firstForAlice = await _host.GetAsync("/ratelimit/a", user: "alice");
        using HttpResponseMessage secondForAlice = await _host.GetAsync("/ratelimit/a", user: "alice");
        using HttpResponseMessage firstForBob = await _host.GetAsync("/ratelimit/a", user: "bob");

        Assert.Multiple(() =>
        {
            Assert.That(firstForAlice.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(secondForAlice.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(firstForBob.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    [Test]
    public async Task RateLimit_SameCallerOnTwoHosts_ShouldNotShareState()
    {
        using HttpResponseMessage firstOnFirstHost = await _host.GetAsync("/ratelimit/a", user: "alice");

        await using RateLimitTestHost otherHost = await RateLimitTestHost.StartAsync();
        using HttpResponseMessage firstOnOtherHost = await otherHost.GetAsync("/ratelimit/a", user: "alice");

        Assert.Multiple(() =>
        {
            Assert.That(firstOnFirstHost.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(firstOnOtherHost.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    #endregion Partitions

    #region Response contract

    [Test]
    public async Task RateLimit_Rejection_ShouldReturnJsonErrorResult()
    {
        using HttpResponseMessage first = await _host.GetAsync("/ratelimit/a");
        using HttpResponseMessage rejected = await _host.GetAsync("/ratelimit/a");

        using JsonDocument document = JsonDocument.Parse(await rejected.Content.ReadAsStringAsync());
        JsonElement root = document.RootElement;

        Assert.Multiple(() =>
        {
            Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(rejected.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

            Assert.That(GetProperty(root, "httpStatusCode").GetInt32(), Is.EqualTo(429));
            Assert.That(GetProperty(root, "message").GetString(), Is.Not.Empty);
        });
    }

    [Test]
    public async Task RateLimit_Rejection_ShouldUseTheConfiguredInternalErrorCode()
    {
        using HttpResponseMessage first = await _host.GetAsync("/ratelimit/a");
        using HttpResponseMessage rejected = await _host.GetAsync("/ratelimit/a");

        using JsonDocument document = JsonDocument.Parse(await rejected.Content.ReadAsStringAsync());

        Assert.That(GetProperty(document.RootElement, "internalCode").GetInt64(), Is.EqualTo(RateLimitTestHost.RateLimitInternalErrorCode));
    }

    [Test]
    public async Task RateLimit_Rejection_ShouldLocalizeTheMessageWithTheDelay()
    {
        using HttpResponseMessage first = await _host.GetAsync("/ratelimit/a");
        using HttpResponseMessage rejected = await _host.GetAsync("/ratelimit/a");

        using JsonDocument document = JsonDocument.Parse(await rejected.Content.ReadAsStringAsync());
        JsonElement root = document.RootElement;

        string expectedMessage = string.Format(CultureInfo.InvariantCulture, TestErrorLocalizerService.RateLimitMessageTemplate, RateLimitTestController.LongRateLimitMilliSeconds);

        Assert.Multiple(() =>
        {
            Assert.That(GetProperty(root, "message").GetString(), Is.EqualTo(expectedMessage));
            Assert.That(GetProperty(root, "metadata")[0].GetInt32(), Is.EqualTo(RateLimitTestController.LongRateLimitMilliSeconds));
        });
    }

    #endregion Response contract

    #region Attribute

    [Test]
    public void EndpointRateLimitAttribute_WithZeroMilliSeconds_ShouldBeAcceptedAsNoLimit()
    {
        EndpointRateLimitAttribute attribute = new() { MilliSeconds = 0 };

        Assert.That(attribute.MilliSeconds, Is.Zero);
    }

    [Test]
    public void EndpointRateLimitAttribute_WithoutMilliSeconds_ShouldDefaultToZero()
    {
        EndpointRateLimitAttribute attribute = new();

        Assert.That(attribute.MilliSeconds, Is.Zero);
    }

    [Test]
    public void EndpointRateLimitAttribute_WithValidMilliSeconds_ShouldKeepTheValue()
    {
        EndpointRateLimitAttribute attribute = new() { MilliSeconds = 250 };

        Assert.That(attribute.MilliSeconds, Is.EqualTo(250));
    }

    #endregion Attribute

    #region Helpers

    /// <summary>
    /// Case-insensitive on purpose: these tests check the rate limiting behavior, not the JSON casing policy
    /// </summary>
    internal static JsonElement GetProperty(JsonElement element, string name)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        throw new KeyNotFoundException($"The property '{name}' was not found in: {element}");
    }

    #endregion Helpers
}
