using Bacon.ApiUtilities.Models;
using Bacon.ApiUtilities.Samples.Attributes.Validations;

namespace Bacon.ApiUtilities.Samples.ProjectConfigs;

/// <summary>
/// 
/// </summary>
public class ApiExceptionHandlerSettings
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Dictionary<ValidationAttributeTypes, long> GetBuiltInValidationAttributeSettings()
    {
        return new Dictionary<ValidationAttributeTypes, long>()
        {
            { ValidationAttributeTypes.AllowedMaxLengthAttribute, 412001 },
            { ValidationAttributeTypes.AllowedMinLengthAttribute, 412002 },
            { ValidationAttributeTypes.AllowedRangeAttribute, 412003 },
            { ValidationAttributeTypes.DuplicatedItemsAttribute, 412004 },
            { ValidationAttributeTypes.EmailFormatAttribute, 412005 },
            { ValidationAttributeTypes.EmptyGuidAttribute, 412006 },
            { ValidationAttributeTypes.RequiredEnumerableContentAttribute, 412007 },
            { ValidationAttributeTypes.RequiredFieldAttribute, 412008 },
            { ValidationAttributeTypes.UtcDateTimeFormatAttribute, 412009 }
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Dictionary<Type, IEnumerable<long>> GetCustomValidationattributeSettings()
    {
        return new Dictionary<Type, IEnumerable<long>>()
        {
            { typeof(AllowedCaptionAttribute), [412010, 412011, 412012, 412014] },
            { typeof(AnotherValidationAttribute), [412013] },
        };
    }
}