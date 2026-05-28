namespace InvoiceSaaS.Application.Common;

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 50;

    public int Page { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public bool SortDescending { get; set; } = true;
}
