using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PopaDin.Bkd.Infra.Documents;

public class RecordDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Name { get; set; } = "";
    public int UserId { get; set; }
    public int Operation { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Value { get; set; }

    public int Frequency { get; set; }
    public List<RecordTagSubDocument> Tags { get; set; } = [];
    public DateTime ReferenceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RecordTagSubDocument
{
    public int OriginalTagId { get; set; }
    public string Name { get; set; } = "";
    public int? TagType { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
}
