using AutoMapper;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarnManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnimalController : ControllerBase
    {
        private readonly IAnimalRepository animalRepository;
        private readonly IMapper mapper;

        public AnimalController(IAnimalRepository animalRepository, IMapper mapper)
        {
            this.animalRepository = animalRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
         [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
          [FromQuery] int pageNumber=1, [FromQuery] int pageSize=1000)
        {
            var userId = GetUserId();
            var domain = await animalRepository.GetAllAnimalsAsync(userId, filterOn,filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize);
            domain = domain.Where(a => a.Farm != null && a.Farm.UserId == userId).ToList();
            var dto = mapper.Map<List<AnimalDto>>(domain);
            return Ok(dto);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var userId = GetUserId();
            var domain = await animalRepository.GetAnimalByIdAsync(id);
            if (domain == null || domain.Farm == null || domain.Farm.UserId != userId) return NotFound();
            var dto = mapper.Map<AnimalDto>(domain);
            return Ok(dto);
        }

        [HttpPost("buy")]
        public async Task<IActionResult> Create([FromBody] AnimalAddDto addDto)
        {
            var userId = GetUserId();
            var domain = mapper.Map<Animal>(addDto);
            domain = await animalRepository.BuyAnimalAsync(domain);
            if (domain == null)
            {
                return BadRequest("You do not have sufficient budget to purchase this animal.");
            }
            var dto = mapper.Map<AnimalDto>(domain);
            return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] AnimalUpdateDto updateDto)
        {
            var userId = GetUserId();
            var existing = await animalRepository.GetAnimalByIdAsync(id);
            if (existing == null || existing.Farm == null || existing.Farm.UserId != userId) return NotFound();
            var domain = mapper.Map<Animal>(updateDto);
            domain = await animalRepository.UpdateAnimalAsync(id, domain);
            var dto = mapper.Map<AnimalDto>(domain);
            return Ok(dto);
        }

        [HttpPost("sell/{id:Guid}")]
        public async Task<IActionResult> Sell([FromRoute] Guid id)
        {
            var userId = GetUserId();
            var existing = await animalRepository.GetAnimalByIdAsync(id);
            if (existing == null || existing.Farm == null || existing.Farm.UserId != userId) return NotFound();
            var domain = await animalRepository.SellAnimalAsync(id);
            var dto = mapper.Map<AnimalDto>(domain);
            return Ok(dto);
        }
        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {   
            var domain = await animalRepository.DeleteAnimalAsync(id);

            if (domain == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<AnimalDto>(domain));
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdStr!);
        }
    }
}
