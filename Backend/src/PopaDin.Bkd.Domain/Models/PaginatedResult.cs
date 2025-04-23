namespace PopaDin.Bkd.Domain.Models;

public class PaginatedResult<T>
{
    public List<T> Lines { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public int TotalItens { get; set; }
    public int TotalPages { get; set; }
}