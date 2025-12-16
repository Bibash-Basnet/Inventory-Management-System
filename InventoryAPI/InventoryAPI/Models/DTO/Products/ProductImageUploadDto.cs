using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace InventoryAPI.Models.DTO
{
    public class ProductImageUploadDto
    {
        public List<IFormFile> Images { get; set; } = new();
    }
}
