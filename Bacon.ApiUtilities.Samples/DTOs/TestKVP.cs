using Bacon.ApiUtilities.Attributes.Validations;
using Bacon.ApiUtilities.Samples.Attributes.Validations;

namespace Bacon.ApiUtilities.Samples.DTOs;

/// <summary>
/// 
/// </summary>
public class TestKVP
{
    /// <summary>
    /// 
    /// </summary>
    [RequiredEnumerableContent, AllowedMaxLength(100)]
    public IEnumerable<KeyValuePair<string, string>>? Values { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [RequiredEnumerableContent, AllowedCaption(50)]
    public IEnumerable<CaptionSet>? Captions { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<int, long>? TestDic { get; set; }
}