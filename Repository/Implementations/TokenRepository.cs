// Token Repository Implementation- Handles JWT creation for authentication
// Provides abstraction layer for generating JWT tokens with user claims and roles

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
        // Dependencies for animal operations
        private readonly IConfiguration configuration; // Access appsettings.json or environment variables
        private static readonly HashSet<string> _blacklistedTokens = new(); // In-memory token blacklist
        private static readonly HashSet<string> _blacklistedUsers = new(); // In-memory user blacklist

        public TokenRepository(IConfiguration configuration)
        {
            this.configuration = configuration; 
        }

        // Creates a JWT token for a given IdentityUser with optional roles
        public string CreateJWTToken(IdentityUser user, List<string> roles)
        {
            // Create user claims (user ID and email)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), // User ID claim
                new Claim(ClaimTypes.Email, user.Email ?? user.UserName ?? string.Empty) // Email claim
            };

            // Ensure roles list is not null
            if (roles == null) roles = new List<string>();

            // Get JWT signing key from configuration
            var signingKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException(
                "JWT signing key is not configured (Jwt:Key)."
            );
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

            // Create signing credentials
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create JWT token with claims, issuer, audience, expiration, and signing credentials
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],   
                audience: configuration["Jwt:Audience"],
                claims: claims,     
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

        // Serialize and return token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Add token to blacklist (invalidate token)
    public void BlacklistToken(string token)
    {
        _blacklistedTokens.Add(token);
    }

    // Check if token is blacklisted
    public bool IsTokenBlacklisted(string token)
    {
        return _blacklistedTokens.Contains(token);
    }

    // Blacklist all tokens for a specific user
    public void BlacklistUserTokens(string userId)
    {
        _blacklistedUsers.Add(userId);
    }

    // Check if user is blacklisted (for additional validation)
    public bool IsUserBlacklisted(string userId)
    {
        return _blacklistedUsers.Contains(userId);
    }
}
}