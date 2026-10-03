using Bacon.ApiUtilities.Models.Apis.Documentations;
using Bacon.ApiUtilities.Models.Apis.RateLimits;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Bacon.ApiUtilities.Interfaces.Services.Apis;

internal interface IEndpointMetadataService
{
    EndpointInformation GetEndpointInformation(ApiDescription apiDescription, ModelStateValidationOptions modelStateValidationOptions, RateLimitOptions rateLimitOptions);
}