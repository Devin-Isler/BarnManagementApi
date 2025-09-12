using AutoMapper;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace BarnManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnimalController : ControllerBase
    {
        private readonly IAnimalRepository animalRepository;
        private readonly IMapper mapper;
        private readonly ILogger<AnimalController> logger;

        public AnimalController(IAnimalRepository animalRepository, IMapper mapper, ILogger<AnimalController> logger)
        {
            this.animalRepository = animalRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
         [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
          [FromQuery] int pageNumber=1, [FromQuery] int pageSize=1000)
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/animal/search requested by User {UserId} with filterOn={FilterOn}, sortBy={SortBy}, asc={Asc}, page={Page}, size={Size}", userId, filterOn, sortBy, isAscending ?? true, pageNumber, pageSize);
            var domain = await animalRepository.GetAllAnimalsAsync(userId, filterOn,filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize);
            var dto = mapper.Map<List<AnimalDto>>(domain);
            logger.LogInformation("GET /api/animal/search returned {Count} animals for User {UserId}", dto.Count, userId);
            return Ok(dto);
        }

        [HttpGet("search/{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/animal/seach/{Id} requested by User {UserId}", id, userId);
            var domain = await animalRepository.GetAnimalByIdAsync(id);
            if (domain == null || domain.Farm == null || domain.Farm.UserId != userId)
            {
                logger.LogWarning("Animal {Id} not found or not accessible by User {UserId}", id, userId);
                return NotFound();
            }
            var dto = mapper.Map<AnimalDto>(domain);
            logger.LogInformation("GET /api/animal/seach/{Id} succeeded for User {UserId}", id, userId);
            return Ok(dto);
        }

        [HttpPost("buy")]
        public async Task<IActionResult> Create([FromBody] AnimalBuyDto addDto)
        {
            var userId = GetUserId();
            logger.LogInformation("POST /api/animal/buy requested by User {UserId} with template={Template} farmId={FarmId}", userId, addDto.Name, addDto.FarmId);
            var domain = await animalRepository.BuyAnimalByTemplateNameAsync(addDto.Name, addDto.FarmId);
            if (domain == null)
            {
                logger.LogWarning("Animal buy failed for User {UserId}: invalid template or insufficient budget. template={Template} farmId={FarmId}", userId, addDto.Name, addDto.FarmId);
                return BadRequest("Invalid template name or insufficient budget.");
            }
            var dto = mapper.Map<AnimalDto>(domain);
            logger.LogInformation("Animal {AnimalId} purchased successfully by User {UserId}", domain.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
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
        /*
        [HttpPut("update/{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] AnimalUpdateDto updateDto)
        {
            var userId = GetUserId();
            logger.LogInformation("PUT /api/animal/update/{Id} requested by User {UserId}", id, userId);
            var existing = await animalRepository.GetAnimalByIdAsync(id);
            if (existing == null || existing.Farm == null || existing.Farm.UserId != userId)
            {
                logger.LogWarning("Animal {Id} not found or not accessible for update by User {UserId}", id, userId);
                return NotFound();
            }
            var domain = mapper.Map<Animal>(updateDto);
            domain = await animalRepository.UpdateAnimalAsync(id, domain);
            var dto = mapper.Map<AnimalDto>(domain);
            logger.LogInformation("Animal {Id} updated successfully by User {UserId}", id, userId);
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
        */

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdStr!);
        }
    }
}
