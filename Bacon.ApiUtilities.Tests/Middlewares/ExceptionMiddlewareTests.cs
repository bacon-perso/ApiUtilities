using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Tests.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace Bacon.ApiUtilities.Tests.Middlewares;

[TestFixture]
internal sealed class ExceptionMiddlewareTests
{
    private const long InternalServerErrorCode = 500000;
    private const long CustomErrorCode = 409001;
    private const string PartialBody = "partial-body";

    private WebApplication _webApplication = null!;
    private HttpClient _client = null!;
    private CapturingLoggerProvider _loggerProvider = null!;

    [SetUp]
    public async Task SetUp()
    {
        _loggerProvider = new();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(_loggerProvider);

        builder.Services.AddSingleton<IErrorLocalizerService, TestErrorLocalizerService>();
        builder.Services.AddControllers()
            .AddApiExceptionHandler<TestResources>(o =>
            {
                o.InternalServerErrorInternalErrorCode = InternalServerErrorCode;
                o.UnhandledValidationInternalErrorCode = 412000;
            });

        _webApplication = builder.Build();

        _webApplication.UseApiExceptionHandler();

        _webApplication.Map("/throw-before-response", _ => throw new InvalidOperationException("boom"));
        _webApplication.Map("/throw-custom", _ => throw new CustomException(HttpStatusCode.Conflict, CustomErrorCode));
        _webApplication.Map("/throw-bad-request", _ => throw new BadHttpRequestException("malformed request", StatusCodes.Status400BadRequest));
        _webApplication.MapGet("/bind-parameter", (int id) => id);
        _webApplication.Map("/throw-after-response-started", async httpContext =>
        {
            await httpContext.Response.WriteAsync(PartialBody);
            await httpContext.Response.Body.FlushAsync();

            throw new InvalidOperationException("boom after start");
        });

        await _webApplication.StartAsync();

        _client = _webApplication.GetTestClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        _client.Dispose();

        await _webApplication.StopAsync();
        await _webApplication.DisposeAsync();

        _loggerProvider.Dispose();
    }

    [Test]
    public async Task UnhandledException_BeforeResponseStarted_ShouldReturnInternalServerErrorResult()
    {
        using HttpResponseMessage response = await _client.GetAsync("/throw-before-response");

        ErrorResult? errorResult = JsonSerializer.Deserialize<ErrorResult>(await response.Content.ReadAsStringAsync(), JsonSerializerOptions.Web);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(errorResult, Is.Not.Null);
            Assert.That(errorResult!.HttpStatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
            Assert.That(errorResult.InternalCode, Is.EqualTo(InternalServerErrorCode));
            Assert.That(ErrorLogs(), Has.Count.EqualTo(1));
        }
    }

    /// <summary>
    /// Guards the log suppression: a handled exception must be logged once, by ApiExceptionHandler, and not a second time by the framework's ExceptionHandlerMiddleware
    /// </summary>
    [Test]
    public async Task UnhandledException_BeforeResponseStarted_ShouldBeLoggedOnlyOnceAcrossAllCategories()
    {
        using HttpResponseMessage response = await _client.GetAsync("/throw-before-response");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(_loggerProvider.Logs.Count(w => w.Level >= LogLevel.Error), Is.EqualTo(1));
            Assert.That(FrameworkErrorLogs(), Is.Empty);
        }
    }

    [Test]
    public async Task CustomException_BeforeResponseStarted_ShouldReturnCustomErrorResult()
    {
        using HttpResponseMessage response = await _client.GetAsync("/throw-custom");

        ErrorResult? errorResult = JsonSerializer.Deserialize<ErrorResult>(await response.Content.ReadAsStringAsync(), JsonSerializerOptions.Web);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(errorResult, Is.Not.Null);
            Assert.That(errorResult!.InternalCode, Is.EqualTo(CustomErrorCode));
        }
    }

    /// <summary>
    /// Characterization test: a framework BadHttpRequestException (400) thrown while the pipeline runs is currently swallowed
    /// by the catch-all and reported as an internal server error. If the handler is changed to let framework exceptions through, this test must be updated
    /// </summary>
    [Test]
    public async Task BadHttpRequestException_BeforeResponseStarted_ShouldCurrentlyReturnInternalServerErrorResult()
    {
        using HttpResponseMessage response = await _client.GetAsync("/throw-bad-request");

        ErrorResult? errorResult = JsonSerializer.Deserialize<ErrorResult>(await response.Content.ReadAsStringAsync(), JsonSerializerOptions.Web);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(errorResult, Is.Not.Null);
            Assert.That(errorResult!.InternalCode, Is.EqualTo(InternalServerErrorCode));
            Assert.That(ErrorLogs(), Has.Count.EqualTo(1));
            Assert.That(ErrorLogs()[0].Exception, Is.TypeOf<BadHttpRequestException>());
        }
    }

    /// <summary>
    /// A parameter binding failure in a minimal api is not an exception outside of Development: the framework writes the 400 itself,
    /// so the exception handler is never involved and must not alter the response
    /// </summary>
    [Test]
    public async Task ParameterBindingFailure_ShouldReturnFrameworkBadRequestUntouched()
    {
        using HttpResponseMessage response = await _client.GetAsync("/bind-parameter?id=not-a-number");

        string body = await response.Content.ReadAsStringAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body, Does.Not.Contain(InternalServerErrorCode.ToString()));
            Assert.That(ErrorLogs(), Is.Empty);
        }
    }

    [Test]
    public async Task UnhandledException_AfterResponseStarted_ShouldBeLoggedByTheFrameworkAndNotWriteAnErrorResult()
    {
        string body = string.Empty;
        HttpStatusCode statusCode;

        using (HttpResponseMessage response = await _client.GetAsync("/throw-after-response-started", HttpCompletionOption.ResponseHeadersRead))
        {
            statusCode = response.StatusCode;

            try
            {
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex) when (ex is IOException or HttpRequestException or OperationCanceledException)
            {
                // The aborted response may surface as a read failure depending on the test server
            }
        }

        using (Assert.EnterMultipleScope())
        {
            // The status code was already sent as 200, so it can never be replaced by the error result's 500
            Assert.That(statusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Not.Contain("httpStatusCode").IgnoreCase);
            Assert.That(body, Does.Not.Contain(InternalServerErrorCode.ToString()));
            // Once the response started, ApiExceptionHandler is not invoked: the framework's ExceptionHandlerMiddleware logs the exception and rethrows it
            Assert.That(ErrorLogs(), Is.Empty);
            Assert.That(FrameworkErrorLogs(), Has.Count.EqualTo(1));
            Assert.That(FrameworkErrorLogs()[0].Exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(FrameworkErrorLogs()[0].Exception!.Message, Is.EqualTo("boom after start"));
        }
    }

    private List<CapturedLog> ErrorLogs()
    {
        return [.. _loggerProvider.Logs.Where(w => w.Level == LogLevel.Error && w.Category.EndsWith("ApiExceptionHandler", StringComparison.Ordinal))];
    }

    private List<CapturedLog> FrameworkErrorLogs()
    {
        return [.. _loggerProvider.Logs.Where(w => w.Level == LogLevel.Error && w.Category.EndsWith("ExceptionHandlerMiddleware", StringComparison.Ordinal))];
    }

    private sealed record CapturedLog(string Category, LogLevel Level, Exception? Exception);

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<CapturedLog> Logs { get; } = [];

        public ILogger CreateLogger(string categoryName)
        {
            return new CapturingLogger(categoryName, Logs);
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
                logs.Enqueue(new(category, logLevel, exception));
            }
        }
    }
}
