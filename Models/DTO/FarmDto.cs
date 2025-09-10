using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    public class FarmDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }
    
        public string? Location { get; set; }
    }
    public class FarmUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(500)]
        public string? Location { get; set; }
    }
    public class FarmAddDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? Location { get; set; }
    }
}


