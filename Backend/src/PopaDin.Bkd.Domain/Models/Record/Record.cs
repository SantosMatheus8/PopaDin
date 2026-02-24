using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models.Record;

public class Record
{
    public int? Id { get; set; }
    public OperationEnum Operation { get; set; }
    public double Value { get; set; }
    public int Frequency { get; set; }
    // public int UserId { get; set; }
    // public UserModel User { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    // public List<RecordTagModel> RecordTags { get; set; } = new List<RecordTagModel>();
}