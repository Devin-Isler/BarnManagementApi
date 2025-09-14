// Farm DTOs - Data Transfer Objects for farm-related API operations
// Used for sending farm data between client and server

using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    // DTO for returning farm data to client
    public class FarmDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string Location { get; set; } = string.Empty;
        public List<AnimalDto> Animals { get; set; } = new List<AnimalDto>();
    }
    
    // DTO for updating an existing farm
    public class FarmUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required, MaxLength(500)]
        public string Location { get; set; } = string.Empty;
    }
    
    // DTO for creating a new farm
    public class FarmAddDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required, MaxLength(500)]
        public string Location { get; set; } = string.Empty;
    }
}
