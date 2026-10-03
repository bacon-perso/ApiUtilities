using Bacon.ApiUtilities.Attributes.Apis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bacon.ApiUtilities.Tests.Support;

[ApiController]
[Route("ratelimit")]
public sealed class RateLimitTestController : ControllerBase
{
    public const int LongRateLimitMilliSeconds = 60000;

    public const int ShortRateLimitMilliSeconds = 1000;

    [HttpGet("a")]
    [EndpointRateLimit(MilliSeconds = LongRateLimitMilliSeconds)]
    public IActionResult ActionA()
    {
        return Ok();
    }

    [HttpGet("b")]
    [EndpointRateLimit(MilliSeconds = LongRateLimitMilliSeconds)]
    public IActionResult ActionB()
    {
        return Ok();
    }

    [HttpGet("short")]
    [EndpointRateLimit(MilliSeconds = ShortRateLimitMilliSeconds)]
    public IActionResult ShortRateLimit()
    {
        return Ok();
    }

    [HttpGet("overload")]
    [EndpointRateLimit(MilliSeconds = LongRateLimitMilliSeconds)]
    public IActionResult Overload()
    {
        return Ok();
    }

    [HttpGet("overload/{id:int}")]
    [EndpointRateLimit(MilliSeconds = LongRateLimitMilliSeconds)]
    public IActionResult Overload(int id)
    {
        return Ok(id);
    }

    [HttpGet("free")]
    public IActionResult Free()
    {
        return Ok();
    }

    /// <summary>
    /// MilliSeconds set to 0 on purpose: the endpoint is not rate limited
    /// </summary>
    [HttpGet("zero")]
    [EndpointRateLimit(MilliSeconds = 0)]
    public IActionResult ZeroRateLimit()
    {
        return Ok();
    }

    /// <summary>
    /// MilliSeconds never set (defaults to 0): the endpoint is not rate limited
    /// </summary>
    [HttpGet("unset")]
    [EndpointRateLimit]
    public IActionResult UnsetRateLimit()
    {
        return Ok();
    }

    /// <summary>
    /// Limited by the application's own named policy, not by <see cref="EndpointRateLimitAttribute"/>
    /// </summary>
    [HttpGet("consumer-policy")]
    [EnableRateLimiting(ConsumerRateLimiting.PolicyName)]
    public IActionResult ConsumerPolicy()
    {
        return Ok();
    }

    /// <summary>
    /// No attribute: only limited by the application's own global limiter
    /// </summary>
    [HttpGet("consumer-global")]
    public IActionResult ConsumerGlobal()
    {
        return Ok();
    }
}
