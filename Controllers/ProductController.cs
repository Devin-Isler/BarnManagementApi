using AutoMapper;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarnManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;

        public ProductController(IProductRepository productRepository, IMapper mapper)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var domain = await productRepository.GetAllProductsAsync();
            var dto = mapper.Map<List<ProductDto>>(domain);
            return Ok(dto);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var domain = await productRepository.GetProductByIdAsync(id);
            if (domain == null) return NotFound();
            var dto = mapper.Map<ProductDto>(domain);
            return Ok(dto);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ProductAddDto addDto)
        {
            var domain = mapper.Map<Product>(addDto);
            domain = await productRepository.CreateProductAsync(domain);
            var dto = mapper.Map<ProductDto>(domain);
            return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
        }

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
    }
}


