namespace Zeiss.ProductManagement.Application.Products;

public sealed class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1 && TotalPages > 0;
    public bool HasNextPage => CurrentPage < TotalPages;

    public static PaginatedResult<T> Create(IReadOnlyList<T> items, int currentPage, int pageSize, int totalCount)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
