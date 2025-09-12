using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
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

    public class ProductAddDto
    {
        [Required]
        public Guid AnimalId { get; set; }
    }

    public class ProductUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public Guid? AnimalId { get; set; }
        public DateTime? SoldAt { get; set; }
    }
}


