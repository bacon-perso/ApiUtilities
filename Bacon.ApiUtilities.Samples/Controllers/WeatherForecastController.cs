using Asp.Versioning;
using Bacon.ApiUtilities.Attributes.Apis;
using Bacon.ApiUtilities.Attributes.Validations;
using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Samples.DTOs;
using Bacon.ApiUtilities.Samples.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Bacon.ApiUtilities.Samples.Controllers;

/// <summary>
/// Weather Controller
/// </summary>
[ApiController]
//[Route("weather")]
[Route("v{version:apiVersion}/weather")]
[EndpointTag("Weather 2")]
[EndpointDefinition("sample1")]
//[ApiVersionNeutral]
[ApiVersion(1)]
public class WeatherForecastController : ControllerBase
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
    /// <param name="totos">A required list of totos</param>
    /// <param name="privilege">An optional privilege</param>
    [HttpGet]
    //[BaseCustomAuthorization(Privileges.GetWeather)]
    [Route("{city}")]
    [ApiResponse(typeof(WeatherForecast))]
    [EndpointRateLimit(MilliSeconds = 20000000)]
    [ApiInternalErrorCodes(404001)]
    [HiddenApi]
    [Obsolete("This is dead")]
    public async Task<IActionResult> GetWeatherByCityAsync([FromRoute, RequiredField] string city, [FromQuery, RequiredField] IEnumerable<int>? totos = null, [FromQuery] Privileges? privilege = null)
    {
        _cities.Contains(city, StringComparer.OrdinalIgnoreCase).ThrowCustomExceptionIfFalse(System.Net.HttpStatusCode.NotFound, 404001, city);

        WeatherForecast weatherForecast = new()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = _summaries[Random.Shared.Next(_summaries.Length)]
        };

        return Ok(weatherForecast);
    }

    /// <summary>
    /// Get weather list
    /// </summary>
    /// <remarks>
    /// Get a list of weather
    /// </remarks>
    [HttpGet]
    //[BaseCustomAuthorization(Privileges.GetWeather)]
    [Route("list")]
    [ApiResponse(typeof(ListReturnData<WeatherForecast>))]
    [EndpointRateLimit(MilliSeconds = 0)]
    [HiddenApi]
    public async Task<IActionResult> GetWeatherListAsync()
    {
        List<WeatherForecast> weatherForecasts = [];
        foreach (string _ in _cities)
        {
            weatherForecasts.Add(new()
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = _summaries[Random.Shared.Next(_summaries.Length)]
            });
        }

        ListReturnData<WeatherForecast> listReturnData = new()
        {
            NbFilteredItems = weatherForecasts.Count,
            Items = weatherForecasts
        };

        return Ok(listReturnData);
    }

    /// <summary>
    /// Create weather
    /// </summary>
    /// <remarks>
    /// Create weather by city
    /// </remarks>
    /// <param name="city">The city</param>
    /// <param name="captionSets">An optional list of captionsy</param>
    /// <param name="forms"></param>
    [HttpPost]
    //[BaseCustomAuthorization(Privileges.PostWeather)]
    [Route("{city}")]
    [ApiResponse(typeof(WeatherForecast))]
    [EndpointRateLimit(MilliSeconds = 500)]
    [ApiInternalErrorCodes(409001)]
    [EnableCors("Cloud Flare")]
    //[HiddenApi]
    public async Task<IActionResult> CreateWeatherByCity([FromRoute] string city, [FromQuery, AllowedMaxLength(10)] IEnumerable<int>? someFilters = null, [FromBody, RequiredField] TestKVP? testKVP = null/*, [FromBody, AllowedMaxLength(10)] IEnumerable<CaptionSet>? captionSets = null*//*, [FromForm] IEnumerable<IFormFile>? forms = null*/)
    {
        _cities.Contains(city, StringComparer.OrdinalIgnoreCase).ThrowCustomExceptionIfTrue(System.Net.HttpStatusCode.Conflict, 409001, city);

        WeatherForecast weatherForecast = new()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = _summaries[Random.Shared.Next(_summaries.Length)]
        };

        return Ok(weatherForecast);
    }
}
