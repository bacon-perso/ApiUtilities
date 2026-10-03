using Bacon.ApiUtilities.Interfaces.Services.ModelStateValidations;
using Bacon.ApiUtilities.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Bacon.ApiUtilities.Tests.Support;

/// <summary>
/// Runs a validation attribute with a stub <see cref="IModelValidationService"/>, so the tests only check the attribute's own decision
/// </summary>
internal sealed class ValidationAttributeTestHelper
{
    public const string InvalidMessagePrefix = "invalid:";

    private readonly StubModelValidationService _stub = new();
    private readonly ServiceProvider _serviceProvider;

    public ValidationAttributeTestHelper()
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton<IModelValidationService>(_stub)
            .BuildServiceProvider();
    }

    /// <summary>
    /// The attribute types the attribute reported as invalid
    /// </summary>
    public IReadOnlyList<ValidationAttributeTypes> InvalidResults => _stub.InvalidResults;

    /// <summary>
    /// The result of the attribute for the value. Null when the value is valid
    /// </summary>
    public ValidationResult? Validate(ValidationAttribute attribute, object? value)
    {
        ValidationContext validationContext = new(new object(), _serviceProvider, null) { DisplayName = "Field" };

        return attribute.GetValidationResult(value, validationContext);
    }

    public static string InvalidMessage(ValidationAttributeTypes validationAttributeType)
    {
        return InvalidMessagePrefix + validationAttributeType;
    }

    private sealed class StubModelValidationService : IModelValidationService
    {
        public List<ValidationAttributeTypes> InvalidResults { get; } = [];

        public ValidationResult CreateInvalidModelValidationResult(ValidationContext validationContext, ValidationAttributeTypes validationAttributeType, params object[] errorMessageData)
        {
            InvalidResults.Add(validationAttributeType);

            return new(InvalidMessage(validationAttributeType));
        }

        public ValidationResult CreateInvalidModelValidationResult(ValidationContext validationContext, string errorResourceKey, HttpStatusCode httpStatusCode, long internalErrorCode, params object[] errorMessageData)
        {
            throw new NotSupportedException("The attributes under test only use the validation attribute type overload");
        }
    }
}
