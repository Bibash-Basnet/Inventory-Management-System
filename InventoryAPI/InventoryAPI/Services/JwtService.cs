using InventoryAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InventoryAPI.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user, out DateTime expiresAt)
        {
            var jwtSection = _config.GetSection("Jwt");
            var keyText = jwtSection["Key"];

            byte[] keyBytes = Encoding.UTF8.GetBytes(keyText);
            if (keyBytes.Length < 32)
            {
                using var sha256 = SHA256.Create();
                keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyText));
            }

            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            expiresAt = DateTime.UtcNow.AddHours(2);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("userid", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}