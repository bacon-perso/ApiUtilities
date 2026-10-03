namespace Bacon.ApiUtilities.Attributes.Apis;

/// <summary>
/// Endpoint definition attribute
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EndpointDefinitionAttribute(string endpointDefinitionName) : Attribute
{
    /// <summary>
    /// Open Api documentation definition name
    /// </summary>
    public string EndpointDefinitionName { get; } = endpointDefinitionName;
}