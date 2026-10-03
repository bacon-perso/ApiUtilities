namespace Bacon.ApiUtilities.Models;

#region UI Types

/// <summary>
/// Defines which UI to use
/// </summary>
public enum UiTypes
{
    /// <summary>
    /// Use swashbuckle
    /// </summary>
    Swagger,

    /// <summary>
    /// Use Scalar
    /// </summary>
    Scalar
}

#endregion UI Types

#region ModelStateValidations

/// <summary>
/// Validation Attribute types
/// </summary>
public enum ValidationAttributeTypes
{
    /// <summary>
    /// Max length attribute
    /// </summary>
    AllowedMaxLengthAttribute,

    /// <summary>
    /// Min length attribute
    /// </summary>
    AllowedMinLengthAttribute,

    /// <summary>
    /// Range attribute
    /// </summary>
    AllowedRangeAttribute,

    /// <summary>
    /// No duplicate items attribute
    /// </summary>
    DuplicatedItemsAttribute,

    /// <summary>
    /// Email format attribute
    /// </summary>
    EmailFormatAttribute,

    /// <summary>
    /// No empty guid attribute
    /// </summary>
    EmptyGuidAttribute,

    /// <summary>
    /// Phone number format attribute
    /// </summary>
    PhoneNumberFormatAttribute,

    /// <summary>
    /// Required enumerable content attribute
    /// </summary>
    RequiredEnumerableContentAttribute,

    /// <summary>
    /// Required attribute
    /// </summary>
    RequiredFieldAttribute,

    /// <summary>
    /// Utc date time format attribute
    /// </summary>
    UtcDateTimeFormatAttribute,

    /// <summary>
    /// Url format attribute
    /// </summary>
    UrlFormatAttribute
}

#endregion ModelStateValidations