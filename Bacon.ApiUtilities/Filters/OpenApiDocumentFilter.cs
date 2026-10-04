using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models.Apis;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Bacon.ApiUtilities.Models.Apis.RateLimits;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Bacon.ApiUtilities.Services;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Text;
using JsonObject = System.Text.Json.Nodes.JsonObject;

namespace Bacon.ApiUtilities.Filters;

internal sealed partial class OpenApiDocumentFilter(string documentName) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument openApiDocument, OpenApiDocumentTransformerContext openApiDocumentTransformerContext, CancellationToken cancellationToken)
    {
        ModelStateValidationOptions modelStateValidationOptions = openApiDocumentTransformerContext.ApplicationServices.GetRequiredService<IOptions<ModelStateValidationOptions>>().Value;
        OpenApiDocumentationOptions openApiDocumentationOptions = openApiDocumentTransformerContext.ApplicationServices.GetRequiredService<IOptions<OpenApiDocumentationOptions>>().Value;
        IEndpointMetadataService endpointMetadataService = openApiDocumentTransformerContext.ApplicationServices.GetRequiredService<IEndpointMetadataService>();
        RateLimitOptions rateLimitOptions = openApiDocumentTransformerContext.ApplicationServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;
        ILogger<OpenApiDocumentFilter> logger = openApiDocumentTransformerContext.ApplicationServices.GetRequiredService<ILogger<OpenApiDocumentFilter>>();

        #region Get Metadata

        List<EndpointInformation> endpointInformations = [];
        foreach (ApiDescriptionGroup apiDescriptionGroup in openApiDocumentTransformerContext.DescriptionGroups)
        {
            foreach (ApiDescription apiDescription in apiDescriptionGroup.Items)
            {
                endpointInformations.Add(endpointMetadataService.GetEndpointInformation(apiDescription, modelStateValidationOptions, rateLimitOptions));
            }
        }

        #endregion Get Metadata

        #region Get Resources

        IErrorLocalizerService errorLocalizerService = (IErrorLocalizerService)openApiDocumentTransformerContext.ApplicationServices.GetRequiredService(typeof(IErrorLocalizerService));
        IEnumerable<long> responseCodes = endpointInformations.SelectMany(sm => sm.ResponseCodes).Distinct();

        Dictionary<long, string> resources = [];
        foreach (long responseCode in responseCodes)
        {
            resources.Add(responseCode, errorLocalizerService.GetResourceValue(responseCode.ToString(CultureInfo.InvariantCulture)));
        }

        #endregion Get Resources

        #region Filter Document

        openApiDocument.Components ??= new OpenApiComponents();
        openApiDocument.Components.Schemas = new Dictionary<string, IOpenApiSchema>();

        openApiDocument.Tags = new SortedSet<OpenApiTag>();

        OpenApiSchema errorSchema = await openApiDocumentTransformerContext.GetOrCreateSchemaAsync(typeof(ErrorResult), cancellationToken: cancellationToken);

        List<EndpointInformation> documentedEndpointInformations = [];
        foreach (EndpointInformation endpointInformation in endpointInformations)
        {
            HttpMethod httpMethod = new(endpointInformation.Verb);

            endpointInformation.Route = FindPathKey(openApiDocument, endpointInformation.Route) ?? endpointInformation.Route;

            if (RemoveExcludedEndpoints(openApiDocument, endpointInformation, httpMethod, logger) || RemoveHiddenEndpoint(openApiDocument, endpointInformation, openApiDocumentationOptions, httpMethod))
            {
                continue;
            }

            if (!openApiDocument.Paths.TryGetValue(endpointInformation.Route, out IOpenApiPathItem? pathItem) || pathItem.Operations is null || !pathItem.Operations.TryGetValue(httpMethod, out OpenApiOperation? openApiOperation))
            {
                LogEndpointNotFoundInDocument(logger, endpointInformation.Verb, endpointInformation.Route, documentName);
                continue;
            }

            Dictionary<long, (int, string)> endpointResources = [];
            foreach (long internalErrorCode in endpointInformation.ResponseCodes)
            {
                if (InternalErrorCodesHelper.TryGetHttpStatusCode(internalErrorCode, out HttpStatusCode? httpStatusCode))
                {
                    endpointResources.Add(internalErrorCode, ((int)httpStatusCode.Value, resources[internalErrorCode]));
                }
            }

            DocumentOperation(openApiOperation, endpointInformation);
            DocumentEndpointTag(openApiOperation, openApiDocument, endpointInformation);
            DocumentEndpointResponses(openApiOperation, endpointResources, errorSchema);
            DocumentEndpointExtensions(openApiOperation, endpointInformation);

            documentedEndpointInformations.Add(endpointInformation);
        }

        await OrganizeSchemas(openApiDocument, openApiDocumentTransformerContext, documentedEndpointInformations, errorSchema, cancellationToken);
        OrganizeTags(openApiDocument, documentedEndpointInformations);

        #endregion Filter Document
    }

    #region Privates

    #region Filter Document

    private static string? FindPathKey(OpenApiDocument openApiDocument, string route)
    {
        if (openApiDocument.Paths.ContainsKey(route))
        {
            return route;
        }

        return openApiDocument.Paths.Keys.FirstOrDefault(k => k.Equals(route, StringComparison.OrdinalIgnoreCase));
    }

    private static bool RemoveOperation(OpenApiDocument openApiDocument, string route, HttpMethod httpMethod)
    {
        if (!openApiDocument.Paths.TryGetValue(route, out IOpenApiPathItem? pathItem))
        {
            return false;
        }

        bool isRemoved = pathItem.Operations?.Remove(httpMethod) == true;

        if (pathItem.Operations is null or { Count: 0 })
        {
            openApiDocument.Paths.Remove(route);
        }

        return isRemoved;
    }

    #region Remove Hidden Endpoints

    private static bool RemoveHiddenEndpoint(OpenApiDocument openApiDocument, EndpointInformation endpointInformation, OpenApiDocumentationOptions openApiDocumentationOptions, HttpMethod httpMethod)
    {
        if (openApiDocumentationOptions.DisplayHiddenEndpoints || !endpointInformation.Metadata.IsHidden)
        {
            return false;
        }

        return RemoveOperation(openApiDocument, endpointInformation.Route, httpMethod);
    }

    #endregion Remove Hidden Endpoints

    #region Remove endpoint not in group definition

    private bool RemoveExcludedEndpoints(OpenApiDocument openApiDocument, EndpointInformation endpointInformation, HttpMethod httpMethod, ILogger<OpenApiDocumentFilter> logger)
    {
        bool isEmptyDocument = string.IsNullOrWhiteSpace(endpointInformation.Metadata.DocumentName);

        if (!isEmptyDocument && string.Equals(endpointInformation.Metadata.DocumentName, documentName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (endpointInformation.IsSkipped)
        {
            LogSkippedEndpoint(logger, endpointInformation.Verb, endpointInformation.Route, documentName);
        }

        RemoveOperation(openApiDocument, endpointInformation.Route, httpMethod);
        return true;
    }

    #endregion Remove endpoint not in group definition

    #region Document Operation

    private static void DocumentOperation(OpenApiOperation openApiOperation, EndpointInformation endpointInformation)
    {
        openApiOperation.OperationId = endpointInformation.OperationId;
        openApiOperation.Deprecated = endpointInformation.Metadata.IsDeprecated;
    }

    #endregion Document Operation

    #region Document Endpoint Tag

    private static void DocumentEndpointTag(OpenApiOperation openApiOperation, OpenApiDocument openApiDocument, EndpointInformation endpointInformation)
    {
        openApiOperation.Tags = new SortedSet<OpenApiTagReference>() { new(endpointInformation.Metadata.Tag, openApiDocument) };
    }

    #endregion Document Endpoint Tag

    #region Document Endpoint Responses

    private static void DocumentEndpointResponses(OpenApiOperation openApiOperation, Dictionary<long, (int httpStatusCode, string resource)> endpointResources, OpenApiSchema errorSchema)
    {
        if (endpointResources.Count == 0)
        {
            return;
        }

        openApiOperation.Responses ??= [];

        HashSet<int> httpStatusCodes = [.. endpointResources.Values.Select(s => s.httpStatusCode).Distinct()];

        foreach (int httpStatusCode in httpStatusCodes)
        {
            string statusCode = httpStatusCode.ToString(CultureInfo.InvariantCulture);

            if (!openApiOperation.Responses.ContainsKey(statusCode))
            {
                openApiOperation.Responses.Add(statusCode, new OpenApiResponse
                {
                    Description = GenerateMarkdown(httpStatusCode, endpointResources),
                    Content = new Dictionary<string, OpenApiMediaType>()
                    {
                        [MediaTypeNames.Application.Json] = new OpenApiMediaType()
                        {
                            Schema = errorSchema
                        }
                    }
                });
            }
        }
    }

    private static string GenerateMarkdown(int httpStatusCode, Dictionary<long, (int httpStatusCode, string resource)> endpointResources)
    {
        IEnumerable<long> parsedCodes = [.. endpointResources.Where(w => w.Value.httpStatusCode == httpStatusCode).Select(s => s.Key).OrderBy(o => o)];

        StringBuilder stringBuilder = new();

        foreach (long internalErrorCode in parsedCodes)
        {
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"* {internalErrorCode} : {endpointResources[internalErrorCode].resource}");
        }

        return stringBuilder.ToString();
    }

    #endregion Document Endpoint Responses

    #region Document Endpoint Extensions

    private static void DocumentEndpointExtensions(OpenApiOperation openApiOperation, EndpointInformation endpointInformation)
    {
        openApiOperation.Extensions ??= new Dictionary<string, IOpenApiExtension>();

        #region x-access-rights

        openApiOperation.Extensions["x-access-rights"] = new JsonNodeExtension(new JsonObject()
        {
            ["privileges"] = endpointInformation.Metadata.IsAnonymous ? "Anonymous" : string.Join(", ", endpointInformation.Metadata.Privileges),
            ["privacy"] = endpointInformation.Metadata.PolicyName
        });

        #endregion x-access-rights

        #region time-in-milliseconds

        if (endpointInformation.Metadata.RateLimit is > 0)
        {
            openApiOperation.Extensions["x-ratelimit"] = new JsonNodeExtension(new JsonObject()
            {
                ["time-in-milliseconds"] = (long)endpointInformation.Metadata.RateLimit,
            });
        }

        #endregion time-in-milliseconds
    }

    #endregion Document Endpoint Extensions

    #region Organize Schemas

    private static async Task OrganizeSchemas(OpenApiDocument openApiDocument, OpenApiDocumentTransformerContext openApiDocumentTransformerContext, IEnumerable<EndpointInformation> endpointInformations, OpenApiSchema errorSchema, CancellationToken cancellationToken)
    {
        openApiDocument.Components!.Schemas!.TryAdd(nameof(ErrorResult), errorSchema);

        IEnumerable<KeyValuePair<Type, string>> schemas = [.. endpointInformations.SelectMany(sm => sm.Schemas).Distinct()];

        foreach (KeyValuePair<Type, string> keyValuePair in schemas)
        {
            OpenApiSchema openApiSchema = await openApiDocumentTransformerContext.GetOrCreateSchemaAsync(keyValuePair.Key, cancellationToken: cancellationToken);

            openApiDocument.Components.Schemas!.TryAdd(keyValuePair.Value, openApiSchema);
        }
    }

    #endregion Organize Schemas

    #region Organize Tags

    private static void OrganizeTags(OpenApiDocument openApiDocument, IEnumerable<EndpointInformation> endpointInformations)
    {
        openApiDocument.Tags = new HashSet<OpenApiTag>(endpointInformations.Select(s => s.Metadata.Tag).Distinct(StringComparer.OrdinalIgnoreCase).Select(s => new OpenApiTag() { Name = s }));
    }

    #endregion Organize Tags

    #endregion Filter Document

    #region Logs

    [LoggerMessage(Level = LogLevel.Debug, Message = "Endpoint {Verb} {Route} is not supported by the OpenAPI document filter and is skipped from document '{DocumentName}'")]
    private static partial void LogSkippedEndpoint(ILogger logger, string verb, string route, string documentName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Endpoint {Verb} {Route} is flagged for document '{DocumentName}' but has no matching operation in the generated OpenAPI document, so it is not documented")]
    private static partial void LogEndpointNotFoundInDocument(ILogger logger, string verb, string route, string documentName);

    #endregion Logs

    #endregion Privates
}