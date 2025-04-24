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

    public const string ListBudgets = @"
        SELECT
            b.Id,
            b.Name As Name,
            b.Goal As Goal,
            b.CurrentAmount As CurrentAmount,
            b.FinishAt As FinishAt,
            b.CreatedAt As CreatedAt,
            b.UpdatedAt As UpdatedAt
        FROM Budget b WITH(NOLOCK)
        WHERE 1 = 1";

    public const string Count = @"
      SELECT COUNT(*)
      FROM Budget b WITH(NOLOCK)
      WHERE 1=1";

    public const string FindBudgetById = @"
        SELECT
            b.Id,
            b.Name As Name,
            b.Goal As Goal,
            b.CurrentAmount As CurrentAmount,
            b.FinishAt As FinishAt,
            b.CreatedAt As CreatedAt,
            b.UpdatedAt As UpdatedAt
        FROM Budget b WITH(NOLOCK)
        WHERE b.Id = @BudgetId";

    public const string UpdateBudget = @"
        UPDATE Budget
        SET Name = @Name,
            Goal = @Goal,
            CurrentAmount = @CurrentAmount,
            UpdatedAt = @UpdatedAt
        WHERE Id = @BudgetId";

    public const string DeleteBudget = @"
        DELETE FROM Budget
        WHERE Id = @BudgetId";
}
