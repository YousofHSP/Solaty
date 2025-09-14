using System.IdentityModel.Tokens.Jwt;

namespace Domain.Auth;

public class AccessToken
{
    public string access_token { get; set; }
    public string access_code { get; set; }
    public string refresh_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }

    public AccessToken(JwtSecurityToken securityToken , string accessCode)
    {
        access_token = new JwtSecurityTokenHandler().WriteToken(securityToken);
        token_type = "Bearer";
        expires_in = (int)(securityToken.ValidTo - DateTime.UtcNow).TotalSeconds;
        access_code = accessCode;
    }
}