namespace Bacon.ApiUtilities.Attributes.Apis;

/// <summary>
/// Adds a rate limit on an endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class EndpointRateLimitAttribute : Attribute
{
    /// <summary>
    /// Sets the amount of time between each request in milliseconds
    /// </summary>
    public uint MilliSeconds  { get; set; }
}