namespace PopaDin.Bkd.Infra.Queries;

public static class BudgetQueries
{
  public const string CreateBudget = @"
  INSERT INTO Budget 
          (Name, Goal, CurrentAmount, FinishAt, UserId, CreatedAt, UpdatedAt)
          VALUES 
          (@Name, @Goal, @CurrentAmount, @FinishAt, 2, @CreatedAt, @UpdatedAt)
           ";

}
