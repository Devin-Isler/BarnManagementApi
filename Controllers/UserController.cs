// User Controller - Handles all user-related API operations
// Manages user profile updates, balance adjustments, and account deletion

using System.Security.Claims;
using AutoMapper;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarnManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all user operations
    public class UserController : ControllerBase
    {
        // Dependencies for user operations
        private readonly IUserRepository userRepository; // Domain user data access
        private readonly IMapper mapper; // Object mapping between DTOs and models
        private readonly UserManager<IdentityUser> userManager; // ASP.NET Identity user management
        private readonly ILogger<UserController> logger; // Logging for operations
        private readonly ITokenRepository tokenRepository; // Token management for logout

        public UserController(IUserRepository userRepository, IMapper mapper, UserManager<IdentityUser> userManager, ILogger<UserController> logger, ITokenRepository tokenRepository)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.userManager = userManager;
            this.logger = logger;
            this.tokenRepository = tokenRepository;
        }

        // Get current user's profile information
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/user/me requested by User {UserId}", userId);

            // Get user from database
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning("GET /api/user/me failed: User {UserId} not found", userId);
                return NotFound();
            }

            logger.LogInformation("GET /api/user/me succeeded for User {UserId}", userId);
            return Ok(mapper.Map<UserDto>(user));
        }

        // Update current user's profile (username and/or password)
        [HttpPut("update/me")]
        public async Task<IActionResult> UpdateMe([FromBody] UserUpdateDto request)
        {
            var userId = GetUserId();
            logger.LogInformation("PUT /api/user/update/me requested by User {UserId} with Username={Username}, HasPassword={HasPassword}", 
                userId, request.Username, !string.IsNullOrWhiteSpace(request.Password));

            // Get domain user
            var existing = await userRepository.GetByIdAsync(userId);
            if (existing == null)
            {
                logger.LogWarning("PUT /api/user/update/me failed: User {UserId} not found", userId);
                return NotFound();
            }

            // Get Identity user
            var identityUser = await userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
            {
                logger.LogError("PUT /api/user/update/me failed: Identity user not found for User {UserId}", userId);
                return NotFound("Identity user not found.");
            }

            // Update username if provided
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                identityUser.UserName = request.Username;
                identityUser.Email = request.Username;
                existing.Username = request.Username;
            }

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                identityUser.PasswordHash = userManager.PasswordHasher.HashPassword(identityUser, request.Password);
                existing.PasswordHash = identityUser.PasswordHash ?? existing.PasswordHash;
            }

            // Update Identity user
            var identityResult = await userManager.UpdateAsync(identityUser);
            if (!identityResult.Succeeded)
            {
                logger.LogError("PUT /api/user/update/me failed for User {UserId}. Identity errors: {Errors}", userId, identityResult.Errors);
                return BadRequest(identityResult.Errors);
            }

            // Update domain user
            existing.UpdatedAt = DateTime.UtcNow;
            var updated = await userRepository.UpdateAsync(existing);
            if (updated == null)
            {
                logger.LogError("PUT /api/user/update/me failed at domain DB for User {UserId}", userId);
                return NotFound();
            }

            logger.LogInformation("PUT /api/user/update/me succeeded for User {UserId}", userId);
            return Ok(mapper.Map<UserDto>(updated));
        }

        // Set user's balance 
        [HttpPost("balance")]
        public async Task<IActionResult> SetBalance([FromBody] AdjustBalanceDto request)
        {
            var userId = GetUserId();
            logger.LogInformation("POST /api/user/balance requested by User {UserId} with Amount={Amount}", userId, request.Amount);

            // Set user balance
            var user = await userRepository.SetBalanceAsync(userId, request.Amount);
            if (user == null)
            {
                logger.LogWarning("POST /api/user/balance failed: User {UserId} not found", userId);
                return NotFound();
            }

            logger.LogInformation("POST /api/user/balance succeeded for User {UserId}. NewBalance={Balance}", userId, user.Balance);
            return Ok(mapper.Map<UserDto>(user));
        }

        // Delete user account and all associated data
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser()
        {
            // Get user Id
            var id = GetUserId();
            logger.LogInformation("DELETE /api/user requested by User {id}", id);

            // Check if user exists
            var user = await userRepository.GetByIdAsync(id);
            if (user == null)
            {
                logger.LogWarning("DELETE /api/user/{Id} failed: User not found", id);
                return NotFound();
            }

            // Blacklist all tokens for this user to force logout
            tokenRepository.BlacklistUserTokens(id.ToString());
            logger.LogInformation("Blacklisted all tokens for User {Id} during account deletion", id);

            // Delete from Identity database first
            var identityUser = await userManager.FindByIdAsync(id.ToString());
            if (identityUser != null)
            {
                var result = await userManager.DeleteAsync(identityUser);
                if (!result.Succeeded)
                {
                    logger.LogError("DELETE /api/user/{Id} failed. Identity errors: {Errors}", id, result.Errors);
                    return BadRequest(result.Errors);
                }
            }

            // Delete from domain database (cascades to farms, animals, products)
            var (success, farmsCount, animalsCount, productsCount) = await userRepository.DeleteUserAsync(id);

            logger.LogInformation("DELETE /api/user/{Id} succeeded. Deleted: {Farms} farms, {Animals} animals, {Products} products. User automatically logged out.", 
                id, farmsCount, animalsCount, productsCount);
            return Ok(mapper.Map<UserDto>(user));
        }

        // Helper method to get current user ID from JWT token
        private Guid GetUserId()
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(idStr!);
        }
    }
}
