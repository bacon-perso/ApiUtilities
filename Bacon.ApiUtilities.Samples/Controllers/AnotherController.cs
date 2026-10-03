using Asp.Versioning;
using Bacon.ApiUtilities.Attributes.Apis;
using Bacon.ApiUtilities.Attributes.Validations;
using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Samples.Attributes.Authorization;
using Bacon.ApiUtilities.Samples.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bacon.ApiUtilities.Samples.Controllers;

/// <summary>
/// Weather Controller
/// </summary>
[ApiController]
//[Route("weather")]
[Route("another")]
[EndpointTag("Another Controller yay")]
[EndpointDefinition("sample1")]
[ApiVersionNeutral]
public class AnotherController : ControllerBase
{
    private static readonly string[] _summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private static readonly string[] _cities = ["montreal", "toronto", "vancouver"];

    /// <summary>
    /// Get weather
    /// </summary>
    /// <remarks>
    /// Get weather by city
    /// </remarks>
    /// <param name="city">The city</param>
    /// <param name="totos">An optional list of totos</param>
    //[HttpGet("Toto")]
    [HttpGet]
    [BaseCustomAuthorization(Privileges.GetWeather)]
    [Route("{city}")]
    [ApiResponse(typeof(AnotherWeatherForecast))]
    [EndpointRateLimit(MilliSeconds = 50)]
    [ApiInternalErrorCodes(404001)]
    [HiddenApi]
    [Obsolete(message: "This endpoint will be removed in a future version")]
    public async Task<IActionResult> GetAnotherByCityAsync([FromRoute, RequiredField] string city, [FromQuery, RequiredField] IEnumerable<int>? totos = null)
    {
        _cities.Contains(city, StringComparer.OrdinalIgnoreCase).ThrowCustomExceptionIfFalse(System.Net.HttpStatusCode.NotFound, 404001, city);

        AnotherWeatherForecast anotherWeatherForecast = new()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = _summaries[Random.Shared.Next(_summaries.Length)]
        };

        return Ok(anotherWeatherForecast);
    }
}