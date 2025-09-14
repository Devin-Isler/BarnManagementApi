// Farm Controller - Handles all farm-related API operations
// Manages farm creation, updating, deletion, and searching functionality

using AutoMapper;
using BarnManagementApi.Data;
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
    [Authorize] // Require authentication for all farm operations
    public class FarmController : ControllerBase
    {
        // Dependencies for farm operations
        private readonly IFarmRepository farmRepository; // Data access layer
        private readonly BarnDbContext context; // Database context
        private readonly IMapper mapper; // Object mapping between DTOs and models
        private readonly ILogger<FarmController> logger; // Logging for operations

        public FarmController(IFarmRepository farmRepository, BarnDbContext context, IMapper mapper, ILogger<FarmController> logger)
        {
            this.farmRepository = farmRepository;
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
        }
        
        // Get all farms with filtering, sorting and pagination
        [HttpGet("search")]
        public async Task<IActionResult> GetAllFarms([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
         [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
          [FromQuery] int pageNumber=1, [FromQuery] int pageSize=1000)
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/farm/search requested by User {UserId} with filterOn={FilterOn}, sortBy={SortBy}, asc={Asc}, page={Page}, size={Size}", userId, filterOn, sortBy, isAscending ?? true, pageNumber, pageSize);
            
            // Get farms from database with filters
            var barnDomain = await farmRepository.GetFarmsByUserAsync(userId, filterOn,filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize);
            
            // Convert to DTOs for API response
            var farmDto = mapper.Map<List<FarmDto>>(barnDomain);

            logger.LogInformation("GET /api/farm/search returned {Count} farms for User {UserId}", farmDto.Count, userId);
            return Ok(farmDto);
        }

        [HttpGet("search/{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var userId = GetUserId();
            logger.LogInformation("GET /api/farm/search/{Id} requested by User {UserId}", id, userId);
            var farmDomain = await farmRepository.GetFarmByIdAsync(id);

            if(farmDomain == null || farmDomain.UserId != userId)
            {
                logger.LogWarning("Farm {Id} not found or not accessible by User {UserId}", id, userId);
                return NotFound();
            }
            // Convert Domain Models to DTOs
            var farmDto = mapper.Map<FarmDto>(farmDomain);
            // Return DTO back to client
            logger.LogInformation("GET /api/farm/search/{Id} succeeded for User {UserId}", id, userId);
            return Ok(farmDto);
        }
 
        // Create a new farm
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FarmAddDto farmAddDto)
        {   
            var userId = GetUserId();
            logger.LogInformation("POST /api/farm requested by User {UserId}", userId);
            
            // Convert DTO to domain model and set owner
            var farmDomainModel = mapper.Map<Farm>(farmAddDto);
            farmDomainModel.UserId = userId;

            // Save farm to database
            farmDomainModel = await farmRepository.CreateFarmAsync(farmDomainModel);

            // Convert back to DTO for response
            var farmDto = mapper.Map<FarmDto>(farmDomainModel);
            logger.LogInformation("Farm {Id} created successfully by User {UserId}", farmDomainModel.Id, userId);
            return CreatedAtAction(nameof(GetById), new {id=farmDomainModel.Id }, farmDto);
        }

        // Update an existing farm
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] FarmUpdateDto farmUpdateDto)
        {
            var userId = GetUserId();
            logger.LogInformation("PUT /api/farm/{Id} requested by User {UserId}", id, userId);
            var existing = await farmRepository.GetFarmByIdAsync(id); // Get farm from database
            if (existing == null || existing.UserId != userId) // Check if farm exists and belongs to user
            {
                logger.LogWarning("Farm {Id} not found or not accessible for update by User {UserId}", id, userId);
                return NotFound();
            }
            // Convert DTO to domain model
            var toUpdate = mapper.Map<Farm>(farmUpdateDto);
            var updated = await farmRepository.UpdateFarmAsync(id, toUpdate); // Update farm in database
            var farmDto = mapper.Map<FarmDto>(updated);
            logger.LogInformation("Farm {Id} updated successfully by User {UserId}", id, userId);
            return Ok(farmDto);
        }

        // Delete a farm
        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var userId = GetUserId();
            logger.LogInformation("DELETE /api/farm/{Id} requested by User {UserId}", id, userId);
            var existing = await farmRepository.GetFarmByIdAsync(id);
            if (existing == null || existing.UserId != userId) // Check if farm exists and belongs to user
            {
                logger.LogWarning("Farm {Id} not found or not accessible for deletion by User {UserId}", id, userId);
                return NotFound();
            }
            
            var (deletedFarm, animalsCount, productsCount) = await farmRepository.DeleteFarmAsync(id);
            if (deletedFarm == null)
            {
                logger.LogWarning("Farm {Id} deletion failed for User {UserId}", id, userId);
                return NotFound();
            }

            logger.LogInformation("Farm {Id} deleted successfully by User {UserId}. Deleted: {Animals} animals, {Products} products", 
                id, userId, animalsCount, productsCount);
            return Ok(mapper.Map<FarmDto>(deletedFarm));
        }

        // Helper method to get current user ID from JWT token
        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdStr!);
        }
    }
}
