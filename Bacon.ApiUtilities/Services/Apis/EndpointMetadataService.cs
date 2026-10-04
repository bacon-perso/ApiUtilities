using Bacon.ApiUtilities.Attributes.Apis;
using Bacon.ApiUtilities.Interfaces.Services.Apis;
using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Bacon.ApiUtilities.Models.Apis.RateLimits;
using Bacon.ApiUtilities.Models.ModelStateValidations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Bacon.ApiUtilities.Services.Apis;

internal sealed class EndpointMetadataService(IModelMetadataProvider modelMetadataProvider) : IEndpointMetadataService
{
    private static readonly FrozenDictionary<string, ValidationAttributeTypes> _validationAttributeTypesByName = Enum.GetValues<ValidationAttributeTypes>().ToFrozenDictionary(v => v.ToString(), StringComparer.OrdinalIgnoreCase);

    public EndpointInformation GetEndpointInformation(ApiDescription apiDescription, ModelStateValidationOptions modelStateValidationOptions, RateLimitOptions rateLimitOptions)
    {
        string verb = apiDescription.HttpMethod?.ToUpperInvariant() ?? throw new InvalidOperationException($"The endpoint '{apiDescription.ActionDescriptor.DisplayName}' has no HTTP method");
        string relativePath = apiDescription.RelativePath ?? throw new InvalidOperationException($"The endpoint '{apiDescription.ActionDescriptor.DisplayName}' has no route");
        relativePath = (relativePath.StartsWith('/') ? "" : "/") + relativePath;

        //skip any non controller based apis
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return new()
            {
                OperationId = string.Empty,
                Verb = verb,
                Route = relativePath,
                ResponseCodes = [],
                Schemas = [],
                IsSkipped = true,
                Metadata = new()
                {
                    DocumentName = string.Empty,
                    IsAnonymous = false,
                    IsHidden = false,
                    PolicyName = string.Empty,
                    Tag = string.Empty,
                    RateLimit = null,
                    IsDeprecated = false,
                    Privileges = []
                }
            };
        }

        HashSet<long> responseCodes =
        [
            modelStateValidationOptions.InternalServerErrorInternalErrorCode,
            modelStateValidationOptions.UnhandledValidationInternalErrorCode
        ];

        EndpointMetadataCollection endpointMetadataCollection = new(apiDescription.ActionDescriptor.EndpointMetadata);

        GetResponseCodeFromRateLimit(endpointMetadataCollection, rateLimitOptions, responseCodes);

        GetResponseCodesFromResponseCodeAttribute(endpointMetadataCollection, responseCodes);

        #region Get response code from validation attributes and get schemas

        Dictionary<Type, string> schemas = [];
        HashSet<Type> validatorTypes = [];

        foreach (ApiParameterDescription apiParameterDescription in apiDescription.ParameterDescriptions)
        {
            if (apiParameterDescription.ModelMetadata is null)
            {
                continue;
            }

            IterateModel(apiParameterDescription.ModelMetadata, schemas, validatorTypes);
        }

        foreach (ApiResponseType apiResponseType in apiDescription.SupportedResponseTypes)
        {
            ModelMetadata? responseMetadata = GetResponseModelMetadata(apiResponseType);

            if (responseMetadata is null)
            {
                continue;
            }

            IterateModel(responseMetadata, schemas, validatorTypes);
        }

        AddValidatorResponseCodes(validatorTypes, modelStateValidationOptions, responseCodes);

        #endregion Get response code from validation attributes and get schemas

        return new()
        {
            OperationId = controllerActionDescriptor.ActionName,
            Verb = verb,
            Route = relativePath,
            ResponseCodes = [.. responseCodes.Order()],
            Schemas = schemas,
            IsSkipped = false,
            Metadata = GetMetadata(controllerActionDescriptor, endpointMetadataCollection)
        };
    }

    #region Private

    #region Rate Limit

    private static void GetResponseCodeFromRateLimit(EndpointMetadataCollection endpointMetadataCollection, RateLimitOptions rateLimitOptions, HashSet<long> responseCodes)
    {
        if (endpointMetadataCollection.GetMetadata<EndpointRateLimitAttribute>() is { MilliSeconds: > 0} && rateLimitOptions is { RateLimitInternalErrorCode: > 0})
        {
            responseCodes.Add(rateLimitOptions.RateLimitInternalErrorCode);
        }
    }

    #endregion Rate Limit

    #region Response Code Attributes

    private static void GetResponseCodesFromResponseCodeAttribute(EndpointMetadataCollection endpointMetadataCollection, HashSet<long> responseCodes)
    {
        IEnumerable<long>? internalErrorCodes = endpointMetadataCollection.GetMetadata<ApiInternalErrorCodesAttribute>()?.InternalErrorCodes;

        if (internalErrorCodes != null)
        {
            responseCodes.UnionWith(internalErrorCodes);
        }
    }

    #endregion Response Code Attributes

    #region Model walk (validation attributes and schemas)

    private static void IterateModel(ModelMetadata root, Dictionary<Type, string> schemaIds, HashSet<Type> validatorTypes)
    {
        Stack<ModelMetadata> pending = new();
        HashSet<Type> expandedTypes = [];

        pending.Push(root);

        while (pending.TryPop(out ModelMetadata? current))
        {
            CollectValidators(current, validatorTypes);

            // A collection is documented through its element type. Nested collections (List of List of T) come back through this branch
            if (current.ElementMetadata is not null)
            {
                pending.Push(current.ElementMetadata);
                continue;
            }

            // A dictionary is documented inline by the framework (additionalProperties): the pair has no schema, only its value can
            if (IsKeyValuePair(current.ModelType))
            {
                PushProperties(current, pending);
                continue;
            }

            if (!TryGetSchemaType(current, out Type? schemaType))
            {
                continue;
            }

            schemaIds.TryAdd(schemaType, GetSchemaId(schemaType));

            if (expandedTypes.Add(schemaType))
            {
                PushProperties(current, pending);
            }
        }
    }

    private static void CollectValidators(ModelMetadata modelMetadata, HashSet<Type> validatorTypes)
    {
        foreach (object validator in modelMetadata.ValidatorMetadata)
        {
            validatorTypes.Add(validator.GetType());
        }
    }

    private static void PushProperties(ModelMetadata modelMetadata, Stack<ModelMetadata> pending)
    {
        foreach (ModelMetadata property in modelMetadata.Properties)
        {
            pending.Push(property);
        }
    }

    private static bool IsKeyValuePair(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
    }

    /// <summary>
    /// Enums and complex types get a schema. Simple types (string, numbers, dates, Guid...), object and non generic structs do not
    /// </summary>
    private static bool TryGetSchemaType(ModelMetadata modelMetadata, [NotNullWhen(true)] out Type? schemaType)
    {
        Type type = Nullable.GetUnderlyingType(modelMetadata.ModelType) ?? modelMetadata.ModelType;

        schemaType = null;

        if (type.IsEnum)
        {
            schemaType = type;
            return true;
        }

        if (!modelMetadata.IsComplexType || type == typeof(object) || (type.IsValueType && !type.IsGenericType))
        {
            return false;
        }

        schemaType = type;
        return true;
    }

    /// <summary>
    /// Mirrors the default schema reference id of the framework: ListReturnData of WeatherForecast becomes ListReturnDataOfWeatherForecast
    /// </summary>
    private static string GetSchemaId(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string name = type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)];

        return $"{name}Of{string.Join("And", type.GetGenericArguments().Select(GetSchemaId))}";
    }

    private ModelMetadata? GetResponseModelMetadata(ApiResponseType apiResponseType)
    {
        if (apiResponseType.ModelMetadata != null)
        {
            return apiResponseType.ModelMetadata;
        }

        if (apiResponseType.Type == null || apiResponseType.Type == typeof(void))
        {
            return null;
        }

        return modelMetadataProvider.GetMetadataForType(apiResponseType.Type);
    }

    private static void AddValidatorResponseCodes(IEnumerable<Type> validatorTypes, ModelStateValidationOptions modelStateValidationOptions, HashSet<long> responseCodes)
    {
        foreach (Type validatorType in validatorTypes)
        {
            if (modelStateValidationOptions.BuiltInValidationAttributeSettings is { Count: > 0 } builtInValidationAttributes
                && _validationAttributeTypesByName.TryGetValue(validatorType.Name, out ValidationAttributeTypes validationAttributeType)
                && builtInValidationAttributes.TryGetValue(validationAttributeType, out long internalErrorCode))
            {
                responseCodes.Add(internalErrorCode);
            }

            if (modelStateValidationOptions.CustomValidationAttributeSettings != null && modelStateValidationOptions.CustomValidationAttributeSettings.TryGetValue(validatorType, out IEnumerable<long>? internalErrorCodes))
            {
                responseCodes.UnionWith(internalErrorCodes);
            }
        }
    }

    #endregion Model walk (validation attributes and schemas)

    #region Get Metadata

    private static EndpointMetadata GetMetadata(ControllerActionDescriptor controllerActionDescriptor, EndpointMetadataCollection endpointMetadataCollection)
    {
        #region IEndpointAccessPrivileges

        List<string> privileges = [];
        object? endpointAccessPrivilegesValue = controllerActionDescriptor.EndpointMetadata.FirstOrDefault(fd => fd.GetType().GetInterfaces().Any(a => a.IsGenericType && a.GetGenericTypeDefinition().Equals(typeof(IEndpointAccessPrivileges<>))));
        if (endpointAccessPrivilegesValue != null)
        {
            IList listPrivileges = (IList)endpointAccessPrivilegesValue.GetType().GetProperty("Privileges")!.GetValue(endpointAccessPrivilegesValue, null)!;

            foreach (object o in listPrivileges)
            {
                privileges.Add(o.ToString()!);
            }
        }

        #endregion IEndpointAccessPrivileges

        bool isHidden = endpointMetadataCollection.GetMetadata<HiddenApiAttribute>() != null;

        return new()
        {
            Tag = endpointMetadataCollection.GetMetadata<EndpointTagAttribute>()?.Tag ?? controllerActionDescriptor.ControllerName,
            IsAnonymous = endpointMetadataCollection.GetMetadata<IAllowAnonymous>() != null,
            IsDeprecated = endpointMetadataCollection.GetMetadata<ObsoleteAttribute>() != null,
            IsHidden = isHidden,
            DocumentName = endpointMetadataCollection.GetMetadata<EndpointDefinitionAttribute>()?.EndpointDefinitionName ?? string.Empty,
            PolicyName = endpointMetadataCollection.GetMetadata<EnableCorsAttribute>()?.PolicyName ?? (isHidden ? "Private" : "Public"),
            RateLimit = endpointMetadataCollection.GetMetadata<EndpointRateLimitAttribute>()?.MilliSeconds,
            Privileges = [.. privileges.Order()]
        };
    }

    #endregion Get Metadata

    #endregion Private
}