namespace VendorRiskScoring.Application.Common.Models;

public class PaginatedList<T>(List<T> items, int count, int pageIndex, int pageSize)
{
    public List<T> Items { get; } = items;
    private int PageIndex { get; } = pageIndex;
    private int TotalPages { get; } = pageSize == -1 ? 1 : (int)Math.Ceiling(count / (double)pageSize);
    public int TotalCount { get; } = count;
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageIndex,
        int pageSize)
        => await CreateAsync(source, pageIndex, pageSize, CancellationToken.None);

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageIndex,
        int pageSize,
        CancellationToken ct)
    {
        // EF Core query ise (IAsyncQueryProvider varsa) async çalış
        if (source.Provider is IAsyncQueryProvider)
        {
            var count = await source.CountAsync(ct);

            if (pageSize != -1)
                source = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var items = await source.ToListAsync(ct);

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        // Değilse (testteki gibi List<T>.AsQueryable()) sync LINQ kullan
        var countSync = source.Count();

        if (pageSize != -1)
            source = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);

        var itemsSync = source.ToList();

        return new PaginatedList<T>(itemsSync, countSync, pageIndex, pageSize);
    }

    public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}