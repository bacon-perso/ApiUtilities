using Bacon.ApiUtilities.Tests.Support;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace Bacon.ApiUtilities.Tests.RateLimiting;

/// <summary>
/// The rate limiter writes the rejection itself, so the 429 <c>ErrorResult</c> must not depend on where
/// <c>UseApiExceptionHandler</c> is placed, or on whether it is used at all
/// </summary>
[TestFixture]
internal sealed class RateLimitMiddlewareOrderTests
{
    private const string RateLimitLoggerCategory = "Bacon.ApiUtilities.RateLimiting";

    private sealed record CapturedLog(string Category, LogLevel Level, string Message);

    private sealed class CapturingLoggerProvider(ConcurrentQueue<CapturedLog> logs) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CapturingLogger(categoryName, logs);
        }

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(string category, ConcurrentQueue<CapturedLog> logs) : ILogger
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
                logs.Enqueue(new(category, logLevel, formatter(state, exception)));
            }
        }
    }

    private static Action<ILoggingBuilder> Capture(ConcurrentQueue<CapturedLog> logs)
    {
        return builder => builder.AddProvider(new CapturingLoggerProvider(logs));
    }

    private static List<CapturedLog> RateLimitLogs(ConcurrentQueue<CapturedLog> logs)
    {
        return [.. logs.Where(w => w.Category == RateLimitLoggerCategory)];
    }

    #region Response does not depend on the middleware order

    [Test]
    public async Task Rejection_ShouldReturnTheErrorResultWhereverTheExceptionHandlerIs([Values] ExceptionHandlerPlacement placement)
    {
        await using RateLimitTestHost host = await RateLimitTestHost.StartAsync(exceptionHandlerPlacement: placement);

        using HttpResponseMessage first = await host.GetAsync("/ratelimit/a");
        using HttpResponseMessage rejected = await host.GetAsync("/ratelimit/a");

        using JsonDocument document = JsonDocument.Parse(await rejected.Content.ReadAsStringAsync());
        JsonElement root = document.RootElement;

        string expectedMessage = string.Format(CultureInfo.InvariantCulture, TestErrorLocalizerService.RateLimitMessageTemplate, RateLimitTestController.LongRateLimitMilliSeconds);

        Assert.Multiple(() =>
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
            Assert.That(rejected.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            Assert.That(root.GetProperty("httpStatusCode").GetInt32(), Is.EqualTo(429));
            Assert.That(root.GetProperty("internalCode").GetInt64(), Is.EqualTo(RateLimitTestHost.RateLimitInternalErrorCode));
            Assert.That(root.GetProperty("message").GetString(), Is.EqualTo(expectedMessage));
            Assert.That(root.GetProperty("metadata")[0].GetInt32(), Is.EqualTo(RateLimitTestController.LongRateLimitMilliSeconds));
        });
    }

    [Test]
    public async Task Rejection_ShouldSetTheContentLengthOfTheBody([Values] ExceptionHandlerPlacement placement)
    {
        await using RateLimitTestHost host = await RateLimitTestHost.StartAsync(exceptionHandlerPlacement: placement);

        using HttpResponseMessage first = await host.GetAsync("/ratelimit/a");
        using HttpResponseMessage rejected = await host.GetAsync("/ratelimit/a");

        byte[] body = await rejected.Content.ReadAsByteArrayAsync();

        Assert.That(rejected.Content.Headers.ContentLength, Is.EqualTo(body.Length));
    }

    #endregion Response does not depend on the middleware order

    #region Logging

    [Test]
    public async Task Rejection_ShouldLogAWarningWhereverTheExceptionHandlerIs([Values] ExceptionHandlerPlacement placement)
    {
        ConcurrentQueue<CapturedLog> logs = [];

        await using RateLimitTestHost host = await RateLimitTestHost.StartAsync(exceptionHandlerPlacement: placement, configureLogging: Capture(logs));

        using HttpResponseMessage first = await host.GetAsync("/ratelimit/a");

        Assert.That(RateLimitLogs(logs), Is.Empty, "An accepted call must not be logged as a rejection");

        using HttpResponseMessage rejected = await host.GetAsync("/ratelimit/a");

        List<CapturedLog> rateLimitLogs = RateLimitLogs(logs);

        Assert.That(rateLimitLogs, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(rateLimitLogs[0].Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(rateLimitLogs[0].Message, Does.Contain("/ratelimit/a").And.Contain(RateLimitTestHost.RateLimitInternalErrorCode.ToString(CultureInfo.InvariantCulture)));
        });
    }

    [Test]
    public async Task Rejections_ShouldEachBeLoggedEvenThoughTheLoggerIsCreatedOnce()
    {
        ConcurrentQueue<CapturedLog> logs = [];

        await using RateLimitTestHost host = await RateLimitTestHost.StartAsync(configureLogging: Capture(logs));

        using HttpResponseMessage first = await host.GetAsync("/ratelimit/a");
        using HttpResponseMessage secondRejected = await host.GetAsync("/ratelimit/a");
        using HttpResponseMessage thirdRejected = await host.GetAsync("/ratelimit/a");
        using HttpResponseMessage otherEndpointFirst = await host.GetAsync("/ratelimit/b");
        using HttpResponseMessage otherEndpointRejected = await host.GetAsync("/ratelimit/b");

        List<CapturedLog> rateLimitLogs = RateLimitLogs(logs);

        Assert.Multiple(() =>
        {
            Assert.That(rateLimitLogs, Has.Count.EqualTo(3));
            Assert.That(rateLimitLogs.Count(c => c.Message.Contains("/ratelimit/a", StringComparison.Ordinal)), Is.EqualTo(2));
            Assert.That(rateLimitLogs.Count(c => c.Message.Contains("/ratelimit/b", StringComparison.Ordinal)), Is.EqualTo(1));
        });
    }

    /// <summary>
    /// The logger is cached per host. A logger shared between hosts would send the second host's rejection to the first host's providers
    /// </summary>
    [Test]
    public async Task Rejection_ShouldLogToTheProvidersOfItsOwnHostOnly()
    {
        ConcurrentQueue<CapturedLog> firstHostLogs = [];
        ConcurrentQueue<CapturedLog> secondHostLogs = [];

        await using RateLimitTestHost firstHost = await RateLimitTestHost.StartAsync(configureLogging: Capture(firstHostLogs));
        await using RateLimitTestHost secondHost = await RateLimitTestHost.StartAsync(configureLogging: Capture(secondHostLogs));

        // The first host is rejected first, so its logger is created before the second host's
        using HttpResponseMessage firstHostAccepted = await firstHost.GetAsync("/ratelimit/a");
        using HttpResponseMessage firstHostRejected = await firstHost.GetAsync("/ratelimit/a");

        using HttpResponseMessage secondHostAccepted = await secondHost.GetAsync("/ratelimit/b");
        using HttpResponseMessage secondHostRejected = await secondHost.GetAsync("/ratelimit/b");

        List<CapturedLog> firstLogs = RateLimitLogs(firstHostLogs);
        List<CapturedLog> secondLogs = RateLimitLogs(secondHostLogs);

        Assert.Multiple(() =>
        {
            Assert.That(firstLogs, Has.Count.EqualTo(1));
            Assert.That(firstLogs[0].Message, Does.Contain("/ratelimit/a"));
            Assert.That(secondLogs, Has.Count.EqualTo(1));
            Assert.That(secondLogs[0].Message, Does.Contain("/ratelimit/b"));
        });
    }

    #endregion Logging
}
