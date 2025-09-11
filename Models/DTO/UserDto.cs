using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    public class UserDto
    {   
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserUpdateDto
    {
        [MaxLength(50)]
        [Required]
        [DataType(DataType.EmailAddress,ErrorMessage = "“Invalid email address.")]
        public string? Username { get; set; }
        [Required]
        [DataType(DataType.Password, ErrorMessage = "Try another password.")]
        public string? Password { get; set; }
    }

    public class AdjustBalanceDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive.")]

        public decimal Amount { get; set; }
    }
}


