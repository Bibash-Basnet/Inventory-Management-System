using Inventory.Models.DTO.Product;

namespace Inventory.Models
{
    public class PaginatedProductResponse
    {
        public List<ProductViewDto> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}