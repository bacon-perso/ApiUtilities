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

        HashSet<Type> schemaTypes = [];
        HashSet<Type> validatorTypes = [];
        HashSet<Type> expandedForSchemas = [];
        HashSet<Type> expandedForValidators = [];

        foreach (ApiParameterDescription apiParameterDescription in apiDescription.ParameterDescriptions)
        {
            if (apiParameterDescription.ModelMetadata == null)
            {
                continue;
            }

            BindingSource bindingSource = apiParameterDescription.Source;

            if (bindingSource == BindingSource.Body || bindingSource == BindingSource.Form || bindingSource == BindingSource.FormFile)
            {
                IterateModelSchema(apiParameterDescription.ModelMetadata, true, schemaTypes, validatorTypes, expandedForSchemas, expandedForValidators);
            }
            else if (bindingSource == BindingSource.Path || bindingSource == BindingSource.Query)
            {
                AddValidatorTypes(apiParameterDescription.ModelMetadata.ValidatorMetadata, validatorTypes);
                AddPathAndQuerySchemaType(apiParameterDescription.ModelMetadata, schemaTypes);
            }
        }

        // Schema for response types. Validators on a response never produce an error code, so they are not collected
        foreach (ApiResponseType apiResponseType in apiDescription.SupportedResponseTypes)
        {
            ModelMetadata? responseMetadata = GetResponseModelMetadata(apiResponseType);

            if (responseMetadata != null)
            {
                IterateModelSchema(responseMetadata, false, schemaTypes, validatorTypes, expandedForSchemas, expandedForValidators);
            }
        }

        AddValidatorResponseCodes(validatorTypes, modelStateValidationOptions, responseCodes);

        #endregion Get response code from validation attributes and get schemas

        return new()
        {
            OperationId = controllerActionDescriptor.ActionName,
            Verb = verb,
            Route = relativePath,
            ResponseCodes = [.. responseCodes.Order()],
            Schemas = schemaTypes,
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

    /// <summary>
    /// Iterate the model and collects the schema types to document and the types of the validation attributes found.
    /// Each type is expanded only once, so self-referencing types (Node.Children is a List of Node) avoid infinite recursion.
    /// </summary>
    private static void IterateModelSchema(ModelMetadata root, bool collectValidators, HashSet<Type> schemaTypes, HashSet<Type> validatorTypes, HashSet<Type> expandedForSchemas, HashSet<Type> expandedForValidators)
    {
        HashSet<Type> expandedTypes = collectValidators ? expandedForValidators : expandedForSchemas;
        Stack<ModelMetadata> pending = new();

        pending.Push(root);

        while (pending.Count > 0)
        {
            ModelMetadata current = pending.Pop();

            // The validators of the node itself, including the ones on a collection property, are collected before unwrapping the collection
            if (collectValidators)
            {
                AddValidatorTypes(current.ValidatorMetadata, validatorTypes);
            }

            ModelMetadata node = UnwrapCollections(current, collectValidators ? validatorTypes : null);

            if (!TryGetSchemaType(node, out Type? schemaType))
            {
                continue;
            }

            schemaTypes.Add(schemaType);

            if (!expandedTypes.Add(schemaType))
            {
                continue;
            }

            if (collectValidators)
            {
                expandedForSchemas.Add(schemaType);
            }

            foreach (ModelMetadata property in node.Properties)
            {
                pending.Push(property);
            }
        }
    }

    /// <summary>
    /// A collection is documented through its element type. Follows nested collections (List of List of T) down to T.
    /// When validatorTypes is given, the validators of every element level (class level attributes of the element type) are collected on the way
    /// </summary>
    private static ModelMetadata UnwrapCollections(ModelMetadata modelMetadata, HashSet<Type>? validatorTypes)
    {
        ModelMetadata node = modelMetadata;

        while (node.ElementMetadata != null)
        {
            node = node.ElementMetadata;

            if (validatorTypes != null)
            {
                AddValidatorTypes(node.ValidatorMetadata, validatorTypes);
            }
        }

        return node;
    }

    /// <summary>
    /// Enums and complex types get a schema. Simple types (string, numbers, dates, Guid...), object and non generic structs do not
    /// </summary>
    private static bool TryGetSchemaType(ModelMetadata node, [NotNullWhen(true)] out Type? schemaType)
    {
        Type type = Nullable.GetUnderlyingType(node.ModelType) ?? node.ModelType;

        schemaType = null;

        if (type.IsEnum)
        {
            schemaType = type;
            return true;
        }

        if (!node.IsComplexType || type == typeof(object) || (type.IsValueType && !type.IsGenericType))
        {
            return false;
        }

        schemaType = type;
        return true;
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

    private static void AddPathAndQuerySchemaType(ModelMetadata modelMetadata, HashSet<Type> schemaTypes)
    {
        if (TryGetSchemaType(UnwrapCollections(modelMetadata, null), out Type? schemaType))
        {
            schemaTypes.Add(schemaType);
        }
    }

    private static void AddValidatorTypes(IReadOnlyList<object> validators, HashSet<Type> validatorTypes)
    {
        foreach (object validator in validators)
        {
            validatorTypes.Add(validator.GetType());
        }
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