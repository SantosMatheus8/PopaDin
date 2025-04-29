namespace PopaDin.Bkd.Api.Dtos.User;

public class UpdateUserRequest
{
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public double Balance { get; set; }
}