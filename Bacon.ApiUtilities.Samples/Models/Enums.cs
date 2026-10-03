using System.Text.Json.Serialization;

namespace Bacon.ApiUtilities.Samples.Models;

/// <summary>
/// Privileges
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Privileges
{
    /// <summary>
    /// Get weather
    /// </summary>
    GetWeather,

    /// <summary>
    /// Post weather
    /// </summary>
    PostWeather
}