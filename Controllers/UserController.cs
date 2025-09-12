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
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public UserController(IUserRepository userRepository, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        // GET: api/user/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(mapper.Map<UserDto>(user));
        }

        // PUT: api/user
        [HttpPut("update/me")]
        public async Task<IActionResult> UpdateMe([FromBody] UserUpdateDto request)
        {
            var userId = GetUserId();
            var existing = await userRepository.GetByIdAsync(userId);
            if (existing == null) return NotFound();

            // Update Identity user (AuthDb)
            var identityUser = await userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null) return NotFound("Identity user not found.");

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                identityUser.UserName = request.Username;
                identityUser.Email = request.Username; // assuming email as username
                existing.Username = request.Username;
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                // Hash and set password via UserManager
                identityUser.PasswordHash = userManager.PasswordHasher.HashPassword(identityUser, request.Password);
                // mirror hashed value into domain model to avoid plain text storage
                existing.PasswordHash = identityUser.PasswordHash ?? existing.PasswordHash;
            }

            var identityResult = await userManager.UpdateAsync(identityUser);
            if (!identityResult.Succeeded)
            {
                return BadRequest(identityResult.Errors);
            }

            existing.UpdatedAt = DateTime.UtcNow;
            var updated = await userRepository.UpdateAsync(existing);
            if (updated == null) return NotFound();
            return Ok(mapper.Map<UserDto>(updated));
        }

        // POST: api/user/balance
        [HttpPost("balance")]
        public async Task<IActionResult> AdjustBalance([FromBody] AdjustBalanceDto request)
        {
            var userId = GetUserId();
            var user = await userRepository.AdjustBalanceAsync(userId, request.Amount);
            if (user == null) return NotFound();
            return Ok(mapper.Map<UserDto>(user));
        }

        // DELETE: api/user
        [HttpDelete]
        public async Task<IActionResult> DeleteMe()
        {
            var userId = GetUserId();

            // Delete from domain DB
            await userRepository.DeleteUserAsync(userId);

            // Delete from AuthDb (Identity)
            var identityUser = await userManager.FindByIdAsync(userId.ToString());
            if (identityUser != null)
            {
                var result = await userManager.DeleteAsync(identityUser);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }

            return NoContent();
        }

        private Guid GetUserId()
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(idStr!);
        }
    }
}


