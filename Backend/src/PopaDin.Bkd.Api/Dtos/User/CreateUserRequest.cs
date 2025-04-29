namespace PopaDin.Bkd.Api.Dtos.User;

public class CreateUserRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public double Balance { get; set; }
}