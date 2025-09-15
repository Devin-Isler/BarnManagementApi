// Product Controller - Handles all product-related API operations
// Manages product selling, updating, deletion, and searching functionality

using System.Security.Claims;
using AutoMapper;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BarnManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all product operations
    public class ProductController : ControllerBase
    {
        // Dependencies for product operations
        private readonly IProductRepository productRepository; // Data access layer
        private readonly IMapper mapper; // Object mapping between DTOs and models
        private readonly ILogger<ProductController> logger; // Logging for operations

        public ProductController(IProductRepository productRepository, IMapper mapper, ILogger<ProductController> logger)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        // Get all products with filtering, sorting and pagination
        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
         [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
          [FromQuery] int pageNumber=1, [FromQuery] int pageSize=1000)
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/product/search requested by User {UserId} with filterOn={FilterOn}, sortBy={SortBy}, asc={Asc}, page={Page}, size={Size}", userId, filterOn, sortBy, isAscending ?? true, pageNumber, pageSize);
            
            // Get products from database with filters
            var domain = await productRepository.GetAllProductsAsync(userId, filterOn,filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize);
            
            // Convert to DTOs for API response
            var dto = mapper.Map<List<ProductDto>>(domain);
            logger.LogInformation("GET /api/product/search returned {Count} products for User {UserId}", dto.Count, userId);
            return Ok(dto);
        }

        [HttpGet("search/{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            logger.LogInformation("GET /api/product/search/{Id} requested", id);
            var domain = await productRepository.GetProductByIdAsync(id);
            if (domain == null)
            {
                logger.LogWarning("Product {Id} not found", id);
                return NotFound();
            }
            var dto = mapper.Map<ProductDto>(domain);
            logger.LogInformation("GET /api/product/search/{Id} succeeded", id);
            return Ok(dto);
        }
        
        // Sell a product
        [HttpPost("sell/{id:Guid}")]
        public async Task<IActionResult> Sell([FromRoute] Guid id)
        {
            var userId = GetUserId();
            logger.LogInformation("POST /api/product/sell/{Id} requested by User {UserId}", id, userId);
            
            // Check if product exists and belongs to user
            var existing = await productRepository.GetProductByIdAsync(id);
            if (existing == null || existing.Animal == null || existing.Animal.Farm == null || existing.Animal.Farm.UserId != userId) 
            {   
                logger.LogWarning("Product {Id} not found or not accessible for sell by User {UserId}", id, userId);
                return NotFound();
            }
            
            // Sell the product
            var domain = await productRepository.SellProductAsync(id);
            var dto = mapper.Map<ProductDto>(domain);
            logger.LogInformation("Product {Id} sold successfully by User {UserId}", id, userId);
            return Ok(dto);
        }
        // Update a product
        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ProductUpdateDto updateDto)
        {
            var userId = GetUserId();
            logger.LogInformation("PUT /api/product/{Id} requested by User {UserId}", id, userId);
            var existing = await productRepository.GetProductByIdAsync(id);
            if (existing == null || existing.Animal == null || existing.Animal.Farm == null || existing.Animal.Farm.UserId != userId || existing.IsSold) // Check if product exists and belongs to user
            {
                logger.LogWarning("Product {Id} not found or not accessible for update by User {UserId}", id, userId);
                return NotFound();
            }
            // Convert DTO to domain model
            var domain = mapper.Map<Product>(updateDto);
            domain = await productRepository.UpdateProductAsync(id, domain); // Update product in database
            if (domain == null) // Check if product was updated
            {
                logger.LogWarning("Product {Id} update failed for User {UserId}", id, userId);
                return NotFound();
            }
            var dto = mapper.Map<ProductDto>(domain);
            logger.LogInformation("Product {Id} updated successfully by User {UserId}", id, userId);
            return Ok(dto);
        }

        // Delete a product
        [HttpDelete("delete/{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var userId = GetUserId();
            logger.LogInformation("DELETE /api/product/{Id} requested by User {UserId}", id, userId);
            var existing = await productRepository.GetProductByIdAsync(id); 
            if (existing == null || existing.Animal == null || existing.Animal.Farm == null || existing.Animal.Farm.UserId != userId) // Check if product exists and belongs to user
            {
                logger.LogWarning("Product {Id} not found or not accessible for deletion by User {UserId}", id, userId);
                return NotFound();
            }
            var domain = await productRepository.DeleteProductAsync(id);
            if (domain == null) 
            {
                logger.LogWarning("Product {Id} deletion failed for User {UserId}", id, userId);
                return NotFound();
            }

            logger.LogInformation("Product {Id} deleted successfully by User {UserId}", id, userId);
            return Ok(mapper.Map<ProductDto>(domain));
        }
        // Helper method to get current user ID from JWT token
        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdStr!);
        }
        

    }
}
