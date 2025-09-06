using System.ComponentModel.DataAnnotations;

namespace SimpleInventory.Web.Models
{
    public class ProductCreateDto
    {
        [Required]
        [StringLength(32, MinimumLength = 3)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class ProductUpdateDto : ProductCreateDto { }

    public class ProductReadDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}


