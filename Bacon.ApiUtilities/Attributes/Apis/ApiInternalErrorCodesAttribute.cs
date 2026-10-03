namespace Bacon.ApiUtilities.Attributes.Apis;

/// <summary>
/// Class that contains the custom response codes
/// </summary>
/// <param name="internalErrorCodes">Status codes</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ApiInternalErrorCodesAttribute(params long[] internalErrorCodes) : Attribute
{
    /// <summary>
    /// The internal response codes
    /// </summary>
    public IEnumerable<long> InternalErrorCodes { get; } = internalErrorCodes;
}