using System.ComponentModel.DataAnnotations;

namespace SimpleInventory.Web.Models
{
    public class CategoryCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        //Add a list of products
    }

    public class CategoryReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}


