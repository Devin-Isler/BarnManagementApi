// Animal Controller - Handles all animal-related API operations
// Manages animal buying, selling, updating, and searching functionality

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
    [Authorize] // Require authentication for all animal operations
    public class AnimalController : ControllerBase
    {
        // Dependencies for animal operations
        private readonly IAnimalRepository animalRepository; // Data access layer
        private readonly IMapper mapper; // Object mapping between DTOs and models
        private readonly ILogger<AnimalController> logger; // Logging for operations

        public AnimalController(IAnimalRepository animalRepository, IMapper mapper, ILogger<AnimalController> logger)
        {
            this.animalRepository = animalRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        // Get all animals with filtering, sorting and pagination
        [HttpGet("search")]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
         [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
          [FromQuery] int pageNumber=1, [FromQuery] int pageSize=1000)
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/animal/search requested by User {UserId} with filterOn={FilterOn}, sortBy={SortBy}, asc={Asc}, page={Page}, size={Size}", userId, filterOn, sortBy, isAscending ?? true, pageNumber, pageSize);
            
            // Get animals from database with filters
            var domain = await animalRepository.GetAllAnimalsAsync(userId, filterOn,filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize);
            
            // Convert to DTOs for API response
            var dto = mapper.Map<List<AnimalDto>>(domain);
            logger.LogInformation("GET /api/animal/search returned {Count} animals for User {UserId}", dto.Count, userId);
            return Ok(dto);
        }

        // Get a specific animal by ID
        [HttpGet("search/{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/animal/seach/{Id} requested by User {UserId}", id, userId);
            
            // Find animal in database
            var domain = await animalRepository.GetAnimalByIdAsync(id);
            
            // Check if animal exists and belongs to user
            if (domain == null || domain.Farm == null || domain.Farm.UserId != userId)
            {
                logger.LogWarning("Animal {Id} not found or not accessible by User {UserId}", id, userId);
                return NotFound();
            }
            
            // Convert to DTO and return
            var dto = mapper.Map<AnimalDto>(domain);
            logger.LogInformation("GET /api/animal/seach/{Id} succeeded for User {UserId}", id, userId);
            return Ok(dto);
        }

        // Buy a new animal using a template name
        [HttpPost("buy")]
        public async Task<IActionResult> Buy([FromBody] AnimalBuyDto addDto)
        {
            var userId = GetUserId();
            logger.LogInformation("POST /api/animal/buy requested by User {UserId} with template={Template} farmId={FarmId}", userId, addDto.Name, addDto.FarmId);
            
            // Try to buy animal using template name
            var domain = await animalRepository.BuyAnimalByTemplateNameAsync(addDto.Name, addDto.FarmId);
            
            // Check if purchase was successful
            if (domain == null)
            {
                logger.LogWarning("Animal buy failed for User {UserId}: invalid template or insufficient budget. template={Template} farmId={FarmId}", userId, addDto.Name, addDto.FarmId);
                return BadRequest("Invalid template name or insufficient budget.");
            }
            
            // Convert to DTO and return created animal
            var dto = mapper.Map<AnimalDto>(domain);
            logger.LogInformation("Animal {AnimalId} purchased successfully by User {UserId}", domain.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
        }

        // Update an existing animal
        [HttpPut("update/{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] AnimalUpdateDto updateDto)
        {
            var userId = GetUserId();
            logger.LogInformation("PUT /api/animal/update/{Id} requested by User {UserId}", id, userId);
            
            // Check if animal exists and belongs to user
            var existing = await animalRepository.GetAnimalByIdAsync(id);
            if (existing == null || existing.Farm == null || existing.Farm.UserId != userId || existing.IsActive == false)
            {
                logger.LogWarning("Animal {Id} not found or not accessible for update by User {UserId}", id, userId);
                return NotFound();
            }
            
            // Update animal with new data
            var domain = mapper.Map<Animal>(updateDto);
            domain = await animalRepository.UpdateAnimalAsync(id, domain);
            
            // Return updated animal
            var dto = mapper.Map<AnimalDto>(domain);
            logger.LogInformation("Animal {Id} updated successfully by User {UserId}", id, userId);
            return Ok(dto);
        }
        
        // Sell an animal
        [HttpPost("sell/{id:Guid}")]
        public async Task<IActionResult> Sell([FromRoute] Guid id)
        {
            var userId = GetUserId();
            logger.LogInformation("POST /api/animal/sell/{Id} requested by User {UserId}", id, userId);
            
            // Check if animal exists and belongs to user
            var existing = await animalRepository.GetAnimalByIdAsync(id);
            if (existing == null || existing.Farm == null || existing.Farm.UserId != userId) 
            {
                logger.LogWarning("Animal {Id} not found or not accessible for sell by User {UserId}", id, userId);
                return NotFound();
            }
            
            // Sell the animal
            var domain = await animalRepository.SellAnimalAsync(id);
            var dto = mapper.Map<AnimalDto>(domain);
            logger.LogInformation("Animal {Id} sold successfully by User {UserId}", id, userId);
            return Ok(dto);
        }

        // Delete an animal and all its products
        [HttpDelete("delete/{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {   
            var userId = GetUserId();
            logger.LogInformation("DELETE /api/animal/{Id} requested by User {UserId}", id, userId);
            
            // Check if animal exists and belongs to user
            var existing = await animalRepository.GetAnimalByIdAsync(id);
            if (existing == null || existing.Farm == null || existing.Farm.UserId != userId)
            {
                logger.LogWarning("Animal {Id} not found or not accessible for deletion by User {UserId}", id, userId);
                return NotFound();
            }

            // Delete animal and get count of deleted products
            var (deletedAnimal, productsCount) = await animalRepository.DeleteAnimalAsync(id);
            if (deletedAnimal == null)
            {
                logger.LogWarning("Animal {Id} deletion failed for User {UserId}", id, userId);
                return NotFound();
            }

            logger.LogInformation("Animal {Id} deleted successfully by User {UserId}. Deleted: {Products} products", 
                id, userId, productsCount);
            return Ok(mapper.Map<AnimalDto>(deletedAnimal));
        }

        // Helper method to get current user ID from JWT token
        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdStr!);
        }
    }
}
