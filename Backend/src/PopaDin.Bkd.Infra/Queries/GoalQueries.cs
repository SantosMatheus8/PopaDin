namespace PopaDin.Bkd.Infra.Queries;

public static class GoalQueries
{
    public const string CreateGoal = @"
  INSERT INTO Goal
          (Name, TargetAmount, Deadline, UserId, CreatedAt, UpdatedAt)
          OUTPUT
            INSERTED.Id AS Id,
            INSERTED.Name AS Name,
            INSERTED.TargetAmount AS TargetAmount,
            INSERTED.Deadline AS Deadline,
            INSERTED.FinishAt AS FinishAt,
            INSERTED.CreatedAt AS CreatedAt,
            INSERTED.UpdatedAt AS UpdatedAt
          VALUES
          (@Name, @TargetAmount, @Deadline, @UserId, @CreatedAt, @UpdatedAt)
           ";

    public const string ListGoals = @"
        SELECT
            g.Id,
            g.Name As Name,
            g.TargetAmount As TargetAmount,
            g.Deadline As Deadline,
            g.FinishAt As FinishAt,
            g.CreatedAt As CreatedAt,
            g.UpdatedAt As UpdatedAt,
            u.Id AS UserId,
            u.Id AS Id,
            u.Name AS Name,
            u.Email AS Email,
            u.Balance AS Balance,
            u.CreatedAt AS CreatedAt,
            u.UpdatedAt AS UpdatedAt
        FROM Goal g WITH(NOLOCK)
        INNER JOIN [User] u WITH(NOLOCK) ON g.UserId = u.Id
        WHERE 1 = 1";

    public const string Count = @"
      SELECT COUNT(*)
      FROM Goal g WITH(NOLOCK)
      WHERE 1=1";

    public const string FindGoalById = @"
        SELECT
            g.Id,
            g.Name As Name,
            g.TargetAmount As TargetAmount,
            g.Deadline As Deadline,
            g.FinishAt As FinishAt,
            g.CreatedAt As CreatedAt,
            g.UpdatedAt As UpdatedAt,
            u.Id AS UserId,
            u.Id AS Id,
            u.Name AS Name,
            u.Email AS Email,
            u.Balance AS Balance,
            u.CreatedAt AS CreatedAt,
            u.UpdatedAt AS UpdatedAt
        FROM Goal g WITH(NOLOCK)
        INNER JOIN [User] u WITH(NOLOCK) ON g.UserId = u.Id
        WHERE g.Id = @GoalId AND g.UserId = @UserId";

    public const string UpdateGoal = @"
        UPDATE Goal
        SET Name = @Name,
            TargetAmount = @TargetAmount,
            Deadline = @Deadline,
            UpdatedAt = @UpdatedAt
        WHERE Id = @GoalId";

    public const string DeleteGoal = @"
        DELETE FROM Goal
        WHERE Id = @GoalId";

    public const string FinishGoal = @"
        UPDATE Goal
        SET FinishAt = @FinishAt,
            UpdatedAt = @UpdatedAt
        WHERE Id = @GoalId";
}
