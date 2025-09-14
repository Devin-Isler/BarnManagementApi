// Animal DTOs - Data Transfer Objects for animal-related API operations
// Used for sending animal data between client and server

using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    // DTO for returning animal data to client
    public class AnimalDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Lifetime { get; set; }
        public int ProductionInterval { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellPrice { get; set; }
        public bool IsActive { get; set; }
        public Guid FarmId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SoldAt { get; set; }
        public DateTime? LastProductionTime { get; set; }
        public DateTime? DeathTime { get; set; }
        public List<ProductDto> Products { get; set; } = new();
    }

    // DTO for buying a new animal
    public class AnimalBuyDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // AnimalType name

        [Required]
        public Guid FarmId { get; set; }
    }

    // DTO for updating an existing animal
    public class AnimalUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
    }
}
