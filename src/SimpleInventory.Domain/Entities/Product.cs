using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleInventory.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 3)]
        public string Sku { get; set; } = default!;

        [Required]
        public string Name { get; set; } = default!;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}