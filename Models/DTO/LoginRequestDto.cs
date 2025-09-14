// Login DTOs - Data Transfer Objects for login-related API operations
// Used for sending login data between client and server
using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    // DTO for sending login requestdata for client
    public class LoginRequestDto
    {
    [Required]
    [DataType(DataType.EmailAddress, ErrorMessage = "“Invalid email address.")] 
    public required string Username  { get; set; }

    [Required]
    [DataType(DataType.Password, ErrorMessage = "Password must be at least 6 characters long.")]
    public required string Password  { get; set; }
    }
    
    // DTO for returning login response to client
    public class LoginResponseDto
    {
        public string? JwtToken {get; set;}
    }
}
