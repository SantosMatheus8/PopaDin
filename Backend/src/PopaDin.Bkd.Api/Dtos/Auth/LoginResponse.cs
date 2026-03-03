using System.Text.Json.Serialization;

namespace PopaDin.Bkd.Api.Dtos.Auth;

public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";
}
