using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Goal;

public class ListGoalsRequest
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public decimal? TargetAmount { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public GoalOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}
