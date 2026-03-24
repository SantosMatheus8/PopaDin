using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PopaDin.Bkd.Infra.Documents;

public class RecurrenceLogDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string SourceRecordId { get; set; } = "";
    public string GeneratedRecordId { get; set; } = "";
    public DateTime OccurrenceDate { get; set; }
    public DateTime ProcessedAt { get; set; }
}
