
//namespace Inventory.Models.DTO.Product
//{
//    public class ProductViewDto
//    {
//        public int Id { get; set; }
//        public string Name { get; set; } = string.Empty;
//        public string? Description { get; set; }
//        public decimal Price { get; set; }
//        public int Quantity { get; set; }
//        public List<string> ImageUrls { get; set; } = new();
//    }
//}

namespace Inventory.Models.DTO.Product
{
    public class ProductViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}