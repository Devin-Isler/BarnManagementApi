using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.Domain
{
    public class ProductType
    {
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; 

        [Required]
        public decimal DefaultSellPrice { get; set; }
    }
}