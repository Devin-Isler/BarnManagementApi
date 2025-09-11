using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using BarnManagementApi.Models.Domain;

namespace BarnManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {   
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly IUserRepository userRepository;
        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository tokenRepository, IUserRepository userRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.userRepository = userRepository;

        }
        // POST: /api/Auth/Register
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var identityUser = new IdentityUser
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Username 
            };

            var identityResult = await userManager.CreateAsync(identityUser, registerRequestDto.Password);

            if(identityResult.Succeeded)
            {
                // Create domain User matching Identity Id
                var domainUser = new User
                {
                    Id = Guid.Parse(identityUser.Id),
                    Username = identityUser.Email ?? identityUser.UserName ?? string.Empty,
                    PasswordHash = identityUser.PasswordHash ?? string.Empty,
                    Balance = 0
                };
                await userRepository.CreateAsync(domainUser);
                return Ok("User is registered. Please login.");
            }
            return BadRequest(identityResult.Errors);
        }


        // POST: /api/Auth/Login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            
            var user = await userManager.FindByEmailAsync(loginRequestDto.Username) ;

            if(user != null)
            {
                var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                if(checkPasswordResult)
                {   
                    // Get Roles
                    var roles  = await userManager.GetRolesAsync(user);
                    if(roles != null)
                    {
                        // Create Token

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
    }
}
