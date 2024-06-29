using Microsoft.IdentityModel.Tokens;
using OCR_API.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OCR_API
{
    public class JwtTokenHelper
    {
        public JwtSecurityToken ReadToken(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(jwtToken);
            return token;
        }

        public string CreateJwtToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nickname),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthenticationSettings.JwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(AuthenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(AuthenticationSettings.JwtIssuer, AuthenticationSettings.JwtIssuer, claims, expires: expires, signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public bool IsTokenValid(string jwtToken)
        {
            var token = ReadToken(jwtToken);
            return token.ValidTo > DateTime.UtcNow;
        }
    }
}