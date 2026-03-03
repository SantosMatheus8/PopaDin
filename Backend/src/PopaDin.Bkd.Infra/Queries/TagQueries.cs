namespace PopaDin.Bkd.Infra.Queries;

public static class TagQueries
{
    public const string CreateTag = @"
  INSERT INTO Tag
          (Name, TagType, Description, UserId, CreatedAt, UpdatedAt)
          OUTPUT
            INSERTED.Id AS Id,
            INSERTED.Name AS Name,
            INSERTED.TagType AS TagType,
            INSERTED.Description AS Description,
            INSERTED.UserId AS UserId,
            INSERTED.CreatedAt AS CreatedAt,
            INSERTED.UpdatedAt AS UpdatedAt
          VALUES
          (@Name, @TagType, @Description, @UserId, @CreatedAt, @UpdatedAt)
           ";

    public const string ListTags = @"
        SELECT
            t.Id,
            t.Name AS Name,
            t.TagType AS TagType,
            t.Description AS Description,
            t.CreatedAt AS CreatedAt,
            t.UpdatedAt AS UpdatedAt,
            u.Id AS UserId,
            u.Id AS Id,
            u.Name,
            u.Email,
            u.Balance,
            u.CreatedAt,
            u.UpdatedAt
        FROM Tag t WITH(NOLOCK)
        INNER JOIN [User] u WITH(NOLOCK) ON t.UserId = u.Id
        WHERE 1 = 1";

    public const string Count = @"
      SELECT COUNT(*)
      FROM Tag t WITH(NOLOCK)
      WHERE 1=1";

    public const string FindTagsByIds = @"
        SELECT t.Id
        FROM Tag t WITH(NOLOCK)
        WHERE t.Id IN @Ids AND t.UserId = @UserId";

    public const string FindTagById = @"
        SELECT
            t.Id,
            t.Name AS Name,
            t.TagType AS TagType,
            t.Description AS Description,
            t.CreatedAt AS CreatedAt,
            t.UpdatedAt AS UpdatedAt,
            u.Id AS UserId,
            u.Id AS Id,
            u.Name,
            u.Email,
            u.Balance,
            u.CreatedAt,
            u.UpdatedAt
        FROM Tag t WITH(NOLOCK)
        INNER JOIN [User] u WITH(NOLOCK) ON t.UserId = u.Id
        WHERE t.Id = @TagId AND t.UserId = @UserId";

    public const string UpdateTag = @"
        UPDATE Tag
        SET Name = @Name,
            TagType = @TagType,
            Description = @Description,
            UpdatedAt = @UpdatedAt
        WHERE Id = @TagId";

    public const string DeleteTag = @"
        DELETE FROM RecordTag WHERE TagId = @TagId
        DELETE FROM Tag WHERE Id = @TagId";
}
