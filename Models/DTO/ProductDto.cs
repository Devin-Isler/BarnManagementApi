// Product DTOs - Data Transfer Objects for product-related API operations
// Used for sending product data between client and server

using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    // DTO for returning product data to client
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public Guid AnimalId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SoldAt { get; set; }
        public bool IsSold { get; set; }
    }

    // DTO for adding a new product
    public class ProductAddDto
    {
        [Required]
        public Guid AnimalId { get; set; }
    }

    // DTO for updating an existing product
    public class ProductUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public decimal? Price { get; set; }
    }
}
