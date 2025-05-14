using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.User;

public class ListUsersRequest
{
    public int? Id { get; set; }
    public string? Name { get; set; } = "";
    public string? Email { get; set; } = "";
    public double? Balance { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public BudgetOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}