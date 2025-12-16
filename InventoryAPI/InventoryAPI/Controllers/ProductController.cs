using InventoryAPI.Models.DTO;
using InventoryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService service, ILogger<ProductController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // Get all products
        [HttpGet("get")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 8, [FromQuery] string? search = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 8;
                if (pageSize > 50) pageSize = 50;

                var result = await _service.GetAllAsync(pageNumber, pageSize, search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new { message = "An error occurred while retrieving products" });
            }
        }

        //Get product by ID 
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var product = await _service.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the product" });
            }
        }


        //Create a new product 

        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                var roles = User.Claims
                    .Where(c => c.Type.Contains("role"))
                    .Select(c => c.Value)
                    .ToList();

                _logger.LogInformation($"[CREATE PRODUCT] User: {username}, Authenticated: {isAuthenticated}, Roles: {string.Join(", ", roles)}");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for product creation");
                    return BadRequest(ModelState);
                }

                var newProduct = await _service.CreateAsync(dto);
                _logger.LogInformation($"Product created successfully with ID: {newProduct.Id}");

                return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, newProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "An error occurred while creating the product" });
            }
        }

        //Update an existing product 

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid model state for updating product {id}");
                    return BadRequest(ModelState);
                }

                var updated = await _service.UpdateAsync(id, dto);
                if (updated == null)
                {
                    _logger.LogWarning($"Product {id} not found for update");
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }

                _logger.LogInformation($"Product {id} updated successfully");
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {id}");
                return StatusCode(500, new { message = "An error occurred while updating the product" });
            }
        }

        // Delete a product 

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                var deleted = await _service.DeleteAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning($"Product {id} not found for deletion");
                    return NotFound(new { message = $"Product with ID {id} not found" });
                }

                _logger.LogInformation($"Product {id} deleted successfully");
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the product" });
            }
        }

        // Upload images to an existing product 

        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImages(int id, [FromForm] ProductImageUploadDto dto)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { message = "Invalid product ID" });

                if (dto.Images == null || !dto.Images.Any())
                {
                    _logger.LogWarning($"No images provided for product {id}");
                    return BadRequest(new { message = "No images uploaded" });
                }

                _logger.LogInformation($"Uploading {dto.Images.Count} images to product {id}");

                var result = await _service.UploadImagesAsync(id, dto.Images);

                if (!result.Success)
                {
                    _logger.LogWarning($"Image upload failed for product {id}: {result.Message}");
                    return BadRequest(new { message = result.Message });
                }

                _logger.LogInformation($"Successfully uploaded images to product {id}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading images to product {id}");
                return StatusCode(500, new { message = "An error occurred while uploading images" });
            }
        }

        // Delete a  image 

        [HttpDelete("images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            try
            {
                if (imageId <= 0)
                    return BadRequest(new { message = "Invalid image ID" });

                var result = await _service.DeleteImageAsync(imageId);

                if (!result.Success)
                {
                    _logger.LogWarning($"Image {imageId} not found for deletion");
                    return NotFound(new { message = result.Message });
                }

                _logger.LogInformation($"Image {imageId} deleted successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image {imageId}");
                return StatusCode(500, new { message = "An error occurred while deleting the image" });
            }
        }
    }
}