namespace PopaDin.Bkd.Infra.Queries;

public static class UserQueries
{
    public const string CreateUser = @"
  INSERT INTO [User]
          (Name, Email, Password, Balance, CreatedAt, UpdatedAt)
          OUTPUT
            INSERTED.Id AS Id,
            INSERTED.Name AS Name,
            INSERTED.Email AS Email,
            INSERTED.Balance AS Balance,
            INSERTED.ProfilePictureUrl AS ProfilePictureUrl,
            INSERTED.CreatedAt AS CreatedAt,
            INSERTED.UpdatedAt AS UpdatedAt
          VALUES
          (@Name, @Email, @Password, @Balance, @CreatedAt, @UpdatedAt)
           ";

    public const string ListUsers = @"
        SELECT
            u.Id AS Id,
            u.Name AS Name,
            u.Email AS Email,
            u.Balance AS Balance,
            u.ProfilePictureUrl AS ProfilePictureUrl,
            u.CreatedAt AS CreatedAt,
            u.UpdatedAt AS UpdatedAt
        FROM [User] u WITH(NOLOCK)
        WHERE 1 = 1";

    public const string Count = @"
      SELECT COUNT(*)
      FROM [User] u WITH(NOLOCK)
      WHERE 1=1";

    public const string FindUserById = @"
        SELECT
            u.Id AS Id,
            u.Name AS Name,
            u.Email AS Email,
            u.Balance AS Balance,
            u.ProfilePictureUrl AS ProfilePictureUrl,
            u.CreatedAt AS CreatedAt,
            u.UpdatedAt AS UpdatedAt
        FROM [User] u WITH(NOLOCK)
        WHERE u.Id = @UserId";

    public const string UpdateUser = @"
        UPDATE [User]
        SET Name = @Name,
            Password = @Password,
            Balance = @Balance,
            UpdatedAt = @UpdatedAt
        WHERE Id = @UserId";

    public const string DeleteUser = @"
        DELETE FROM [User]
        WHERE Id = @UserId";

    public const string UpdateBalance = @"
        UPDATE [User]
        SET Balance = Balance + @Amount,
            UpdatedAt = @UpdatedAt
        WHERE Id = @UserId";

    public const string SetBalance = @"
        UPDATE [User]
        SET Balance = @Balance,
            UpdatedAt = @UpdatedAt
        WHERE Id = @UserId";

        public const string FindUserByEmail = @"
        SELECT
            u.Id AS Id,
            u.Name AS Name,
            u.Email AS Email,
            u.Password AS Password,
            u.Balance AS Balance,
            u.ProfilePictureUrl AS ProfilePictureUrl,
            u.CreatedAt AS CreatedAt,
            u.UpdatedAt AS UpdatedAt
        FROM [User] u WITH(NOLOCK)
        WHERE u.Email = @UserEmail";

    public const string UpdateProfilePictureUrl = @"
        UPDATE [User]
        SET ProfilePictureUrl = @ProfilePictureUrl,
            UpdatedAt = @UpdatedAt
        WHERE Id = @UserId";
}
