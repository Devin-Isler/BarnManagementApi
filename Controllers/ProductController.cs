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
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;
        private readonly ILogger<ProductController> logger;

        public ProductController(IProductRepository productRepository, IMapper mapper, ILogger<ProductController> logger)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
         [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
          [FromQuery] int pageNumber=1, [FromQuery] int pageSize=1000)
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/product/search requested by User {UserId} with filterOn={FilterOn}, sortBy={SortBy}, asc={Asc}, page={Page}, size={Size}", userId, filterOn, sortBy, isAscending ?? true, pageNumber, pageSize);
            var domain = await productRepository.GetAllProductsAsync(userId, filterOn,filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize);
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
        
        [HttpPost("sell/{id:Guid}")]
        public async Task<IActionResult> Sell([FromRoute] Guid id)
        {
            var userId = GetUserId();
            logger.LogInformation("POST /api/product/sell/{Id} requested by User {UserId}", id, userId);
            var existing = await productRepository.GetProductByIdAsync(id);
            if (existing == null || existing.Animal == null || existing.Animal.Farm == null || existing.Animal.Farm.UserId != userId) 
            {   
                logger.LogWarning("Product {Id} not found or not accessible for sell by User {UserId}", id, userId);
                return NotFound();
            }
            var domain = await productRepository.SellProductAsync(id);
            var dto = mapper.Map<ProductDto>(domain);
            logger.LogInformation("Product {Id} sold successfully by User {UserId}", id, userId);
            return Ok(dto);
        }
        /*
        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> Create([FromBody] ProductAddDto addDto)
        //{
        //    var domain = await productRepository.ProduceProductAsync(addDto.AnimalId);
        //    var dto = mapper.Map<ProductDto>(domain);
        //    return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
        //}

        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ProductUpdateDto updateDto)
        {
            var domain = mapper.Map<Product>(updateDto);
            domain = await productRepository.UpdateProductAsync(id, domain);
            if (domain == null) return NotFound();
            var dto = mapper.Map<ProductDto>(domain);
            return Ok(dto);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var domain = await productRepository.DeleteProductAsync(id);
            if (domain == null) return NotFound();
            var dto = mapper.Map<ProductDto>(domain);
            return Ok(dto);
        }
        */
        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdStr!);
        }
        

    }
}
