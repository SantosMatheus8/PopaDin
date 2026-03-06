namespace PopaDin.Bkd.Api.Dtos.Budget;

public class CreateBudgetRequest
{
    public string Name { get; set; } = "";
    public decimal Goal { get; set; }
}
