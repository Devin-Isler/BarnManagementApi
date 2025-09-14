// Authentication Controller - Handles user registration and login
// Manages JWT token generation and user authentication

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using BarnManagementApi.Models.Domain;
using Microsoft.AspNetCore.Authorization;

namespace BarnManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {   
        // Dependencies for authentication operations
        private readonly UserManager<IdentityUser> userManager; // ASP.NET Identity user management
        private readonly ITokenRepository tokenRepository; // JWT token generation
        private readonly IUserRepository userRepository; // Domain user data access
        
        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository tokenRepository, IUserRepository userRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.userRepository = userRepository;
        }
        // Register a new user account
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            // Validate request data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Create Identity user
            var identityUser = new IdentityUser
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Username 
            };

            // Create user in Identity system
            var identityResult = await userManager.CreateAsync(identityUser, registerRequestDto.Password);

            if(identityResult.Succeeded)
            {
                // Create corresponding domain user with starting balance
                var domainUser = new User
                {
                    Id = Guid.Parse(identityUser.Id),
                    Username = identityUser.Email ?? identityUser.UserName ?? string.Empty,
                    PasswordHash = identityUser.PasswordHash ?? string.Empty,
                    Balance = 0 // Start with zero balance
                };
                await userRepository.CreateAsync(domainUser);
                return Ok("User is registered. Please login.");
            }
            return BadRequest(identityResult.Errors);
        }


        // Login user and generate JWT token
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            // Find user by email/username
            var user = await userManager.FindByEmailAsync(loginRequestDto.Username);

            if(user != null)
            {
                // Verify password
                var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                if(checkPasswordResult)
                {   
                    // Get user roles
                    var roles = await userManager.GetRolesAsync(user);
                    if(roles != null)
                    {
                        // Generate JWT token
                        var jwtToken = tokenRepository.CreateJWTToken(user, roles.ToList());
                        var response = new LoginResponseDto
                        {
                            JwtToken = jwtToken,
                        };

                        return Ok(response);
                    }
                }
            }
            return BadRequest("Wrong Password or Username.");
        }

        // Logout user and invalidate JWT token
        [HttpPost]
        [Route("Logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Get the current user's token from the Authorization header
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return BadRequest("No valid token provided.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("No token provided.");
            }

            // Add token to blacklist to invalidate it
            tokenRepository.BlacklistToken(token);
            
            return Ok("Successfully logged out.");
        }
    }
}
