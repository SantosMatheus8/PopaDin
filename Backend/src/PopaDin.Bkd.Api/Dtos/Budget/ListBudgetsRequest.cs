using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models;

public class ListBudgetsRequest
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public decimal? Goal { get; set; }
    public decimal? CurrentAmount { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public BudgetOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}