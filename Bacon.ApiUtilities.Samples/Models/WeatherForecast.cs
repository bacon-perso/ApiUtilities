using System.Text.Json.Serialization;

namespace Bacon.ApiUtilities.Samples.Models;

/// <summary>
/// A weather forecast
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// A date
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Temperature in Celcius
    /// </summary>
    [JsonPropertyName("tempCelcius")]
    public int TemperatureC { get; set; }

    /// <summary>
    /// Temperature in Fahrenheit
    /// </summary>
    [JsonPropertyName("tempFahrenheit")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// The summary
    /// </summary>
    public string? Summary { get; set; }
}