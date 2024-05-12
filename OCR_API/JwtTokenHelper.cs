using Microsoft.IdentityModel.Tokens;
using OCR_API.Entities;
using OCR_API.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OCR_API
{
    public class JwtTokenHelper
    {
        private readonly AuthenticationSettings authenticationSettings;
        public JwtTokenHelper()
        {
            authenticationSettings = new AuthenticationSettings()
            {
                JwtKey = Environment.GetEnvironmentVariable("JwtKey"),
                JwtExpireDays = int.Parse(Environment.GetEnvironmentVariable("JwtExpireDays")),
                JwtIssuer = Environment.GetEnvironmentVariable("JwtIssuer")
            };
        }

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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(authenticationSettings.JwtIssuer, authenticationSettings.JwtIssuer, claims, expires: expires, signingCredentials: credentials);

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
