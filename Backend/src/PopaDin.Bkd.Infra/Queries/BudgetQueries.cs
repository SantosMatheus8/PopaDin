namespace PopaDin.Bkd.Infra.Queries;

public static class BudgetQueries
{
    public const string CreateBudget = @"
  INSERT INTO Budget 
          (Name, Goal, CurrentAmount, UserId, CreatedAt, UpdatedAt)
          OUTPUT 
            INSERTED.Id AS Id,
            INSERTED.Name AS Name,
            INSERTED.Goal AS Goal,
            INSERTED.CurrentAmount AS CurrentAmount,
            INSERTED.FinishAt AS FinishAt,
            INSERTED.UserId AS UserId,
            INSERTED.CreatedAt AS CreatedAt,
            INSERTED.UpdatedAt AS UpdatedAt
          VALUES 
          (@Name, @Goal, @CurrentAmount, 2, @CreatedAt, @UpdatedAt)
           ";

}
