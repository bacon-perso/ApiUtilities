namespace Bacon.ApiUtilities.Attributes.Apis;

/// <summary>
/// Endpoint tag attribute 
/// </summary>
/// <remarks>
/// Replaces the normal tag attribute as endpoints should be contained within a document.
/// Having a single tag matches the library better as endpoints are controlled by document instead.
/// If the endpoint tag is not used, the controller name will be used instead as per the default tags behavior
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EndpointTagAttribute(string tag) : Attribute
{
    /// <summary>
    /// The endpoint tag
    /// </summary>
    public string Tag { get; set; } = tag;
}