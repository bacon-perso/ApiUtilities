namespace Bacon.ApiUtilities.Models.Apis.Documentations;

internal sealed class EndpointInformation
{
    public required string OperationId { get; init; }

    public required string Verb { get; init; }

    public required string Route { get; set; }

    public required IReadOnlyList<long> ResponseCodes { get; init; }

    public required HashSet<Type> Schemas { get; init; }

    public required EndpointMetadata Metadata { get; init; }

    public required bool IsSkipped { get; init; }
}