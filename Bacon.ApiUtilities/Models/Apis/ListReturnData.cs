namespace Bacon.ApiUtilities.Models.Apis;

/// <summary>
/// Wrapper class when paginated data is returned. Can be inherited if other propeties such as the total number of items is needed
/// </summary>
/// <typeparam name="TItem"></typeparam>
public class ListReturnData<TItem>
{
    /// <summary>
    /// The total number of items included in the filtered list
    /// </summary>
    public long NbFilteredItems { get; set; }

    /// <summary>
    /// A list of objects to return, with paging
    /// </summary>
    public IEnumerable<TItem>? Items { get; set; }
}