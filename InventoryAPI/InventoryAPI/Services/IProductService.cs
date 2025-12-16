using InventoryAPI.Models;
using InventoryAPI.Models.DTO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryAPI.Services
{
    public interface IProductService
    {
        Task<PaginatedProductResponse> GetAllAsync(int pageNumber, int pageSize, string? search = null);
        Task<ProductResponseDto?> GetByIdAsync(int id);
        Task<ProductResponseDto> CreateAsync(ProductCreateDto dto);
        Task<ProductResponseDto?> UpdateAsync(int id, ProductUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ImageUploadResult> UploadImagesAsync(int productId, List<IFormFile> files);
        Task<ImageUploadResult> DeleteImageAsync(int imageId);
    }

    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? ImageUrls { get; set; }
    }

    public class PaginatedProductResponse
    {
        public List<ProductResponseDto> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}