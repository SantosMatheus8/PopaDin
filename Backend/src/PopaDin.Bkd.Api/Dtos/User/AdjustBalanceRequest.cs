using System.ComponentModel.DataAnnotations;

namespace PopaDin.Bkd.Api.Dtos.User;

public class AdjustBalanceRequest
{
    [Required]
    public decimal Balance { get; set; }
}
