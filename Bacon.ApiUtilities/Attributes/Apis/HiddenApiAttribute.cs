namespace Bacon.ApiUtilities.Attributes.Apis;

/// <summary>
/// Custom attribute that will hide the endpoint unless DisplayHiddenEndpoints is true
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class HiddenApiAttribute : Attribute
{
}