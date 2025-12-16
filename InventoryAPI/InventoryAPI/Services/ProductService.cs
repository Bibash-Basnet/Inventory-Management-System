using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ApplicationDbContext db, ILogger<ProductService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<PaginatedProductResponse> GetAllAsync(int pageNumber, int pageSize, string? search = null)
        {
            
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var products = await query
                .Include(p => p.Images)
                .OrderByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Images = p.Images.Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl
                    }).ToList()
                })
                .ToListAsync();

            return new PaginatedProductResponse
            {
                Products = products,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }

        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var p = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return null;

            return new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                Images = p.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }

        // CREATE PRODUCT
        public async Task<ProductResponseDto> CreateAsync(ProductCreateDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Quantity = dto.Quantity
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Product created with ID: {product.Id}");

            if (dto.Images != null && dto.Images.Any())
            {
                _logger.LogInformation($"Uploading {dto.Images.Count} images for product {product.Id}");
                var uploadResult = await UploadImagesAsync(product.Id, dto.Images);

                if (!uploadResult.Success)
                {
                    _logger.LogWarning($"Image upload failed: {uploadResult.Message}");
                }
                else
                {
                    _logger.LogInformation($"Successfully uploaded {uploadResult.ImageUrls?.Count ?? 0} images");
                }
            }

            var result = await GetByIdAsync(product.Id);
            if (result == null)
            {
                _logger.LogError($"Failed to retrieve created product {product.Id}");
                throw new InvalidOperationException($"Product {product.Id} was created but could not be retrieved");
            }

            return result;
        }

        // Product Update
        public async Task<ProductResponseDto?> UpdateAsync(int id, ProductUpdateDto dto)
        {
            var p = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null)
            {
                _logger.LogWarning($"Product with ID {id} not found for update");
                return null;
            }

            p.Name = dto.Name;
            p.Description = dto.Description;
            p.Price = dto.Price;
            p.Quantity = dto.Quantity;

            _logger.LogInformation($"Updating product {id}: Name={dto.Name}, Price={dto.Price}, Quantity={dto.Quantity}");

            if (dto.RemoveImageIds != null && dto.RemoveImageIds.Any())
            {
                _logger.LogInformation($"Removing {dto.RemoveImageIds.Count} images from product {id}");

                foreach (var imgId in dto.RemoveImageIds)
                {
                    var img = p.Images.FirstOrDefault(i => i.Id == imgId);
                    if (img != null)
                    {
                        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
                        if (File.Exists(fullPath))
                        {
                            try
                            {
                                File.Delete(fullPath);
                                _logger.LogInformation($"Deleted image file: {fullPath}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Failed to delete image file {fullPath}: {ex.Message}");
                            }
                        }

                        _db.ProductImages.Remove(img);
                    }
                }
            }

            await _db.SaveChangesAsync();

            // Upload new images if provided
            if (dto.NewImages != null && dto.NewImages.Any())
            {
                _logger.LogInformation($"Uploading {dto.NewImages.Count} new images for product {id}");
                var uploadResult = await UploadImagesAsync(p.Id, dto.NewImages);

                if (!uploadResult.Success)
                {
                    _logger.LogWarning($"Image upload failed: {uploadResult.Message}");
                }
                else
                {
                    _logger.LogInformation($"Successfully uploaded {dto.NewImages.Count} images");
                }
            }

            return await GetByIdAsync(p.Id);
        }


        // DELETE PRODUCT
        public async Task<bool> DeleteAsync(int id)
        {
            var p = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null)
            {
                _logger.LogWarning($"Product with ID {id} not found for deletion");
                return false;
            }

            _logger.LogInformation($"Deleting product {id} with {p.Images.Count} images");

            foreach (var img in p.Images)
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                        _logger.LogInformation($"Deleted image file: {fullPath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to delete image file {fullPath}: {ex.Message}");
                    }
                }
            }

            _db.Products.Remove(p);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Product {id} deleted successfully");
            return true;
        }

        // UPLOAD IMAGES
        public async Task<ImageUploadResult> UploadImagesAsync(int productId, List<IFormFile> files)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
            {
                _logger.LogWarning($"Product {productId} not found for image upload");
                return new ImageUploadResult { Success = false, Message = "Product not found" };
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "product-images");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation($"Created directory: {folderPath}");
            }

            var uploadedUrls = new List<string>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning($"Invalid file extension: {extension}");
                    continue;
                }

                if (file.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning($"File too large: {file.Length} bytes");
                    continue;
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var img = new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = $"/product-images/{fileName}"
                    };

                    _db.ProductImages.Add(img);
                    uploadedUrls.Add(img.ImageUrl);

                    _logger.LogInformation($"Image uploaded: {fileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to upload image {fileName}: {ex.Message}");
                }
            }

            await _db.SaveChangesAsync();

            return new ImageUploadResult
            {
                Success = uploadedUrls.Any(),
                Message = uploadedUrls.Any() ? $"{uploadedUrls.Count} images uploaded successfully" : "No images were uploaded",
                ImageUrls = uploadedUrls
            };
        }

        // DELETE IMAGE
        public async Task<ImageUploadResult> DeleteImageAsync(int imageId)
        {
            var img = await _db.ProductImages.FindAsync(imageId);
            if (img == null)
            {
                _logger.LogWarning($"Image with ID {imageId} not found");
                return new ImageUploadResult { Success = false, Message = "Image not found" };
            }

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.ImageUrl.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Deleted image file: {fullPath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to delete image file {fullPath}: {ex.Message}");
                }
            }

            _db.ProductImages.Remove(img);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"Image {imageId} deleted successfully");
            return new ImageUploadResult { Success = true, Message = "Image deleted successfully" };
        }
    }
}