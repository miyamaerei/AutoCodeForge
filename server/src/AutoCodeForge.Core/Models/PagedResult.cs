namespace AutoCodeForge.Core.Models;

/// <summary>
/// Represents a paged query result.
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of matching records.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the current page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Initializes an empty paged result.
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Initializes a paged result with items and paging metadata.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="totalCount">The total matching record count.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The page size.</param>
    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}