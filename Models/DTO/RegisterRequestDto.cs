using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public required string Username  { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password  { get; set; }

        [Required]
        public required string[] Roles {get; set;}

    }
}