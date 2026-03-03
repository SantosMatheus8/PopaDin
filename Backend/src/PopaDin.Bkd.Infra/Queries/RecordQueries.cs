namespace PopaDin.Bkd.Infra.Queries;

public static class RecordQueries
{
    public const string CreateRecord = @"
  INSERT INTO Record
          (Operation, Value, Frequency, UserId, CreatedAt, UpdatedAt)
          OUTPUT
            INSERTED.Id AS Id,
            INSERTED.Operation AS Operation,
            INSERTED.Value AS Value,
            INSERTED.Frequency AS Frequency,
            INSERTED.UserId AS UserId,
            INSERTED.CreatedAt AS CreatedAt,
            INSERTED.UpdatedAt AS UpdatedAt
          VALUES
          (@Operation, @Value, @Frequency, @UserId, @CreatedAt, @UpdatedAt)
           ";

    public const string ListRecords = @"
        SELECT
            r.Id,
            r.Operation AS Operation,
            r.Value AS Value,
            r.Frequency AS Frequency,
            r.CreatedAt AS CreatedAt,
            r.UpdatedAt AS UpdatedAt,
            u.Id AS UserId,
            u.Id AS Id,
            u.Name,
            u.Email,
            u.Balance,
            u.CreatedAt,
            u.UpdatedAt,
            t.Id AS TagId,
            t.Id AS Id,
            t.Name,
            t.TagType,
            t.Description,
            t.CreatedAt,
            t.UpdatedAt
        FROM Record r WITH(NOLOCK)
        INNER JOIN [User] u WITH(NOLOCK) ON r.UserId = u.Id
        LEFT JOIN RecordTag rt WITH(NOLOCK) ON rt.RecordId = r.Id
        LEFT JOIN Tag t WITH(NOLOCK) ON t.Id = rt.TagId
        WHERE 1 = 1";

    public const string Count = @"
      SELECT COUNT(*)
      FROM Record r WITH(NOLOCK)
      WHERE 1=1";

    public const string FindRecordById = @"
        SELECT
            r.Id,
            r.Operation AS Operation,
            r.Value AS Value,
            r.Frequency AS Frequency,
            r.CreatedAt AS CreatedAt,
            r.UpdatedAt AS UpdatedAt,
            u.Id AS UserId,
            u.Id AS Id,
            u.Name,
            u.Email,
            u.Balance,
            u.CreatedAt,
            u.UpdatedAt,
            t.Id AS TagId,
            t.Id AS Id,
            t.Name,
            t.TagType,
            t.Description,
            t.CreatedAt,
            t.UpdatedAt
        FROM Record r WITH(NOLOCK)
        INNER JOIN [User] u WITH(NOLOCK) ON r.UserId = u.Id
        LEFT JOIN RecordTag rt WITH(NOLOCK) ON rt.RecordId = r.Id
        LEFT JOIN Tag t WITH(NOLOCK) ON t.Id = rt.TagId
        WHERE r.Id = @RecordId AND r.UserId = @UserId";

    public const string UpdateRecord = @"
        UPDATE Record
        SET Operation = @Operation,
            Value = @Value,
            Frequency = @Frequency,
            UpdatedAt = @UpdatedAt
        WHERE Id = @RecordId";

    public const string CreateRecordTag = @"
        INSERT INTO RecordTag (RecordId, TagId) VALUES (@RecordId, @TagId)";

    public const string DeleteRecordTags = @"
        DELETE FROM RecordTag WHERE RecordId = @RecordId";

    public const string DeleteRecord = @"
        DELETE FROM RecordTag WHERE RecordId = @RecordId;
        DELETE FROM Record WHERE Id = @RecordId;";
}
