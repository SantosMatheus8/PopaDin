using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PopaDin.AlertService.Models;

public class AlertRule
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int UserId { get; set; }
    public string Type { get; set; } = "";
    public decimal Threshold { get; set; }
    public string Channel { get; set; } = "";
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}
