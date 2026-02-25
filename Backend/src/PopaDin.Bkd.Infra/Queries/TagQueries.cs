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
          (@Name, @TagType, @Description, 2, @CreatedAt, @UpdatedAt)
           ";

    public const string ListTags = @"
        SELECT
            t.Id,
            t.Name As Name,
            t.TagType As TagType,
            t.Description As Description,
            t.CreatedAt As CreatedAt,
            t.UpdatedAt As UpdatedAt
        FROM Tag t WITH(NOLOCK)
        WHERE 1 = 1";

    public const string Count = @"
      SELECT COUNT(*)
      FROM Tag t WITH(NOLOCK)
      WHERE 1=1";

    public const string FindTagsByIds = @"
        SELECT t.Id
        FROM Tag t WITH(NOLOCK)
        WHERE t.Id IN @Ids";

    public const string FindTagById = @"
        SELECT
            b.Id,
            b.Name As Name,
            b.TagType As TagType,
            b.Description As Description,
            b.CreatedAt As CreatedAt,
            b.UpdatedAt As UpdatedAt
        FROM Tag b WITH(NOLOCK)
        WHERE b.Id = @TagId";

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
