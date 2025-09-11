using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
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