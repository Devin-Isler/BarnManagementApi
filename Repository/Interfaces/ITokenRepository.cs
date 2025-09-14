// Token Repository Interface - Defines data access operations for tokens
// Provides abstraction layer for token-related database operations
using Microsoft.AspNetCore.Identity;

namespace BarnManagementApi.Repository
{
    public interface ITokenRepository
    {
        // Create unique token for user
        string CreateJWTToken(IdentityUser user, List<String> roles);
        
        // Add token to blacklist (invalidate token)
        void BlacklistToken(string token);
        
        // Check if token is blacklisted
        bool IsTokenBlacklisted(string token);
        
        // Blacklist all tokens for a specific user
        void BlacklistUserTokens(string userId);
        
        // Check if user is blacklisted (for additional validation)
        bool IsUserBlacklisted(string userId);
    }
}
