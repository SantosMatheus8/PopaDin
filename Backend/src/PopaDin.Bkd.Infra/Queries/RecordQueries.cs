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
          (@Operation, @Value, @Frequency, 2, @CreatedAt, @UpdatedAt)
           ";

    public const string ListRecords = @"
        SELECT
            r.Id,
            r.Operation As Operation,
            r.Value As Value,
            r.Frequency As Frequency,
            r.CreatedAt As CreatedAt,
            r.UpdatedAt As UpdatedAt
        FROM Record r WITH(NOLOCK)
        WHERE 1 = 1";

    public const string Count = @"
      SELECT COUNT(*)
      FROM Record b WITH(NOLOCK)
      WHERE 1=1";

    public const string FindRecordById = @"
        SELECT
            b.Id,
            b.Operation As Operation,
            b.Value As Value,
            b.Frequency As Frequency,
            b.CreatedAt As CreatedAt,
            b.UpdatedAt As UpdatedAt
        FROM Record b WITH(NOLOCK)
        WHERE b.Id = @RecordId";

    public const string UpdateRecord = @"
        UPDATE Record
        SET Operation = @Operation,
            Value = @Value,
            Frequency = @Frequency,
            UpdatedAt = @UpdatedAt
        WHERE Id = @RecordId";

    public const string DeleteRecord = @"
        DELETE FROM RecordTag WHERE RecordId = @RecordId;
        DELETE FROM Record WHERE Id = @RecordId;";
}
