using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace InventoryAPI.Models.DTO
{
    public class ProductUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public List<IFormFile>? NewImages { get; set; }

        public List<int>? RemoveImageIds { get; set; }
    }
}