namespace Bacon.ApiUtilities.Models.Apis.Documentations;

internal sealed class EndpointMetadata
{
    public required string DocumentName { get; set; }

    public required bool IsAnonymous { get; set; }

    public required bool IsHidden { get; set; }

    public required string PolicyName { get; set; }

    public required string Tag { get; set; }

    public uint? RateLimit { get; set; }

    public required bool IsDeprecated { get; set; }

    public required IEnumerable<string> Privileges { get; set; }
}