using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models.User;

public class ListUsers
{
    public int? Id { get; set; }
    public string? Name { get; set; } = "";
    public string? Email { get; set; } = "";
    public string? Password { get; set; } = "";
    public double? Balance { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public UserOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}