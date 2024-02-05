namespace SimpleErp.Models;

public class TruckFilterOptions
{
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
}
