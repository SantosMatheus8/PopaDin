namespace PopaDin.Bkd.Infra.Queries;

public static class BudgetQueries
{
    public const string CreateBudget = @"
  INSERT INTO Budget
          (Name, Goal, UserId, CreatedAt, UpdatedAt)
          OUTPUT
            INSERTED.Id AS Id,
            INSERTED.Name AS Name,
            INSERTED.Goal AS Goal,
            INSERTED.FinishAt AS FinishAt,
            INSERTED.CreatedAt AS CreatedAt,
            INSERTED.UpdatedAt AS UpdatedAt
          VALUES
          (@Name, @Goal, @UserId, @CreatedAt, @UpdatedAt)
           ";

    public const string ListBudgets = @"
        SELECT
            b.Id,
            b.Name As Name,
            b.Goal As Goal,
            b.FinishAt As FinishAt,
            b.CreatedAt As CreatedAt,
            b.UpdatedAt As UpdatedAt,
            u.Id AS UserId,
            u.Id AS Id,
            u.Name AS Name,
            u.Email AS Email,
            u.Balance AS Balance,
            u.CreatedAt AS CreatedAt,
            u.UpdatedAt AS UpdatedAt
        FROM Budget b WITH(NOLOCK)
        INNER JOIN [User] u WITH(NOLOCK) ON b.UserId = u.Id
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
            b.FinishAt As FinishAt,
            b.CreatedAt As CreatedAt,
            b.UpdatedAt As UpdatedAt,
            u.Id AS UserId,
            u.Id AS Id,
            u.Name AS Name,
            u.Email AS Email,
            u.Balance AS Balance,
            u.CreatedAt AS CreatedAt,
            u.UpdatedAt AS UpdatedAt
        FROM Budget b WITH(NOLOCK)
        INNER JOIN [User] u WITH(NOLOCK) ON b.UserId = u.Id
        WHERE b.Id = @BudgetId AND b.UserId = @UserId";

    public const string UpdateBudget = @"
        UPDATE Budget
        SET Name = @Name,
            Goal = @Goal,
            UpdatedAt = @UpdatedAt
        WHERE Id = @BudgetId";

    public const string DeleteBudget = @"
        DELETE FROM Budget
        WHERE Id = @BudgetId";
}
