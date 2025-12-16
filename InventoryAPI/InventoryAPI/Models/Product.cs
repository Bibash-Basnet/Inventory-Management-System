using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public List<ProductImage> Images { get; set; } = new();
    }
}
