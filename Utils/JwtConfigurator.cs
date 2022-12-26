using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiTraductorPMV.Dtos;

namespace WebApiTraductorPMV.Utils;

public class JwtConfigurator
{
    public static UserTokenDTO BuildToken(UserModel userInfo, List<string> roles, IConfiguration configuration)
    {
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString())
            };

        claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddDays(Convert.ToInt32(configuration["Jwt:ExpirationDays"]));

        JwtSecurityToken token = new JwtSecurityToken(
           issuer: configuration["Jwt:Issuer"],
           audience: configuration["Jwt:Audience"],
           claims: claims,
           expires: expiration,
           signingCredentials: creds);

        return new UserTokenDTO(new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }

    public static int GetTokenIdUsuario(ClaimsIdentity identity)
    {
        if (identity != null)
        {
            IEnumerable<Claim> claims = identity.Claims;
            foreach (var claim in claims)
            {
                if (claim.Type == "idUsuario")
                {
                    return Convert.ToInt32(claim.Value);
                }
            }
        }

        return 0;
    }

}
