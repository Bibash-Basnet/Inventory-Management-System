using Inventory.Models.DTO.Product;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Inventory.Services;
using Inventory.Models;

namespace Inventory.Controllers
{
    public class ProductController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IApiClient apiClient, IHttpContextAccessor httpContextAccessor, ILogger<ProductController> logger)
        {
            _apiClient = apiClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private bool IsUserLoggedIn()
        {
            var token = HttpContext.Session.GetString("JWT");
            return !string.IsNullOrEmpty(token);
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            _logger.LogInformation($"Checking role: {role}");
            return role == "Admin";
        }

        // GET: Product/Index
        public async Task<IActionResult> Index(int pageNumber = 1, string? search = null)
        {
            if (!IsUserLoggedIn())
            {
                _logger.LogWarning("User not logged in, redirecting to login");
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var url = $"Product/get?pageNumber={pageNumber}&pageSize=8";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    url += $"&search={Uri.EscapeDataString(search)}";
                }

                var response = await _apiClient.GetAsync<PaginatedProductResponse>(url);


                ViewBag.SearchQuery = search;

                if (IsAdmin())
                {
                    _logger.LogInformation("Admin user - showing AdminIndex");
                    return View("AdminIndex", response);
                }

                _logger.LogInformation("Regular user - showing Index");
                return View(response);


            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load products: {ex.Message}");
                TempData["Error"] = $"Failed to load products: {ex.Message}";

                var emptyResponse = new PaginatedProductResponse
                {
                    Products = new List<ProductViewDto>(),
                    TotalCount = 0,
                    TotalPages = 0,
                    CurrentPage = 1,
                    PageSize = 8,
                    HasPreviousPage = false,
                    HasNextPage = false
                };

                return IsAdmin() ? View("AdminIndex", emptyResponse) : View(emptyResponse);
            }
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var product = await _apiClient.GetAsync<ProductViewDto>($"Product/{id}");

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load product details: {ex.Message}");
                TempData["Error"] = "Failed to load product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Product/Create (Admin Only)
        public IActionResult Create()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAdmin())
            {
                _logger.LogWarning("Non-admin user attempted to access Create");
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // POST: Product/Create (Admin Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateDto model)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _logger.LogInformation($"Creating product: {model.Name}");

                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(model.Name), "Name");
                formData.Add(new StringContent(model.Price.ToString()), "Price");
                formData.Add(new StringContent(model.Quantity.ToString()), "Quantity");

                if (!string.IsNullOrEmpty(model.Description))
                {
                    formData.Add(new StringContent(model.Description), "Description");
                }

                if (model.Images != null && model.Images.Any())
                {
                    _logger.LogInformation($"Uploading {model.Images.Count} images");
                    foreach (var image in model.Images)
                    {
                        var streamContent = new StreamContent(image.OpenReadStream());
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                        formData.Add(streamContent, "Images", image.FileName);
                    }
                }

                var product = await _apiClient.PostFormDataAsync<ProductViewDto>("Product/add", formData);

                if (product != null)
                {
                    _logger.LogInformation($"Product created successfully: ID {product.Id}");
                    TempData["Success"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to create product.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating product: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = $"Error creating product: {ex.Message}";
                return View(model);
            }
        }

        // GET: Product/Edit/5 (Admin Only)
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var product = await _apiClient.GetAsync<ProductViewDto>($"Product/{id}");

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new ProductUpdateDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    ExistingImages = product.Images
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load product for editing: {ex.Message}");
                TempData["Error"] = "Failed to load product for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Edit/5 (Admin Only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductUpdateDto model)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction(nameof(Index));
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _apiClient.GetAsync<ProductViewDto>($"Product/{id}");
                    if (existingProduct != null)
                    {
                        model.ExistingImages = existingProduct.Images;
                    }
                }
                catch { }

                return View(model);
            }

            try
            {
                using var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(model.Name), "Name");
                formData.Add(new StringContent(model.Price.ToString()), "Price");
                formData.Add(new StringContent(model.Quantity.ToString()), "Quantity");

                if (!string.IsNullOrEmpty(model.Description))
                {
                    formData.Add(new StringContent(model.Description), "Description");
                }

                if (model.NewImages != null && model.NewImages.Any())
                {
                    foreach (var image in model.NewImages)
                    {
                        var streamContent = new StreamContent(image.OpenReadStream());
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                        formData.Add(streamContent, "NewImages", image.FileName);
                    }
                }

                if (model.RemoveImageIds != null && model.RemoveImageIds.Any())
                {
                    foreach (var imageId in model.RemoveImageIds)
                    {
                        formData.Add(new StringContent(imageId.ToString()), "RemoveImageIds");
                    }
                }

                var product = await _apiClient.PutFormDataAsync<ProductViewDto>($"Product/{id}", formData);

                if (product != null)
                {
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to update product.";

                try
                {
                    var existingProduct = await _apiClient.GetAsync<ProductViewDto>($"Product/{id}");
                    if (existingProduct != null)
                    {
                        model.ExistingImages = existingProduct.Images;
                    }
                }
                catch { }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product: {ex.Message}");
                TempData["Error"] = $"Error updating product: {ex.Message}";

                try
                {
                    var existingProduct = await _apiClient.GetAsync<ProductViewDto>($"Product/{id}");
                    if (existingProduct != null)
                    {
                        model.ExistingImages = existingProduct.Images;
                    }
                }
                catch { }

                return View(model);
            }
        }

        // GET: Product/Delete/5 (Admin Only)
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var product = await _apiClient.GetAsync<ProductViewDto>($"Product/{id}");

                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load product: {ex.Message}");
                TempData["Error"] = "Failed to load product.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Delete/5 (Admin Only)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin only.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _apiClient.DeleteAsync($"Product/{id}");

                if (result)
                {
                    _logger.LogInformation($"Product {id} deleted successfully");
                    TempData["Success"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete product.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product: {ex.Message}");
                TempData["Error"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}