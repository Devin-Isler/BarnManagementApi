// Register DTOs - Data Transfer Objects for register-related API operations
// Used for sending register data between client and server
using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    // DTO for sending register data for client
    public class RegisterRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress,ErrorMessage = "“Invalid email address.")]
        public required string Username  { get; set; }

        [Required]
        [DataType(DataType.Password,  ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password  { get; set; }

    }
}