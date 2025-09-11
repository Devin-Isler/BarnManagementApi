using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

namespace BarnManagementApi.Repository
{
    public class TokenRepository : ITokenRepository
    {

        private readonly IConfiguration configuration;
        public TokenRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string CreateJWTToken(IdentityUser user, List<string> roles)
        {
            // Create claims
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Email, user.Email ?? user.UserName ?? string.Empty));

            if (roles == null)
            {
                roles = new List<string>();
            }
            
            var signingKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT signing key is not configured (Jwt:Key).");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                configuration["Jwt:Issuer"],
                configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);
            
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}