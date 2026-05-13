namespace AutoFlow_Backend.Application.Common;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedResponse<TTarget> Map<TTarget>(Func<T, TTarget> mapper) => new()
    {
        Items = Items.Select(mapper).ToList(),
        TotalCount = TotalCount,
        Page = Page,
        PageSize = PageSize
    };
}
