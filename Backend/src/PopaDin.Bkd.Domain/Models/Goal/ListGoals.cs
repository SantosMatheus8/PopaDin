using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models;

public class ListGoals
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public decimal? TargetAmount { get; set; }
    public int UserId { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public GoalOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}
