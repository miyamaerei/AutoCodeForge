using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Core.Helpers;

/// <summary>
/// Provides helpers for paging parameter normalization and result construction.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Normalizes paging parameters and enforces a maximum page size.
    /// </summary>
    /// <param name="page">The requested page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="maxPageSize">The maximum allowed page size.</param>
    /// <returns>The normalized page number and page size.</returns>
    public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = 200)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 20 : pageSize;
        normalizedPageSize = Math.Min(normalizedPageSize, maxPageSize);
        return (normalizedPage, normalizedPageSize);
    }

    /// <summary>
    /// Wraps items and metadata into a paged result.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The items in the current page.</param>
    /// <param name="totalCount">The total matching record count.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The current page size.</param>
    /// <returns>A paged result instance.</returns>
    public static PagedResult<T> ToPagedResult<T>(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        return new PagedResult<T>(items, totalCount, page, pageSize);
    }
}