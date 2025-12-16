using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace InventoryAPI.Models.DTO
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<IFormFile>? Images { get; set; } 
    }
}
