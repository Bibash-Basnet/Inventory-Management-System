//using System.ComponentModel.DataAnnotations;

//namespace Inventory.Models.DTO.Product
//{
//    public class ProductUpdateDto
//    {
//        public int Id { get; set; }

//        [Required(ErrorMessage = "Product name is required")]
//        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
//        public string Name { get; set; } = string.Empty;

//        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
//        public string? Description { get; set; }

//        [Required(ErrorMessage = "Price is required")]
//        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
//        public decimal Price { get; set; }

//        [Required(ErrorMessage = "Quantity is required")]
//        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
//        public int Quantity { get; set; }

//        public List<string> ExistingImageUrls { get; set; } = new();
//        public List<IFormFile>? NewImages { get; set; }
//        public List<int>? RemoveImageIds { get; set; }
//    }
//}
using System.ComponentModel.DataAnnotations;

namespace Inventory.Models.DTO.Product
{
    public class ProductUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        // New images to upload
        public List<IFormFile>? NewImages { get; set; }

        // Image IDs to remove
        public List<int>? RemoveImageIds { get; set; }

        // Existing images (for display)
        public List<ProductImageDto>? ExistingImages { get; set; }
    }
}