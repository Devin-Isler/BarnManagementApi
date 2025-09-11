using AutoMapper;
using BarnManagementApi.Data;
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
    public class FarmController : ControllerBase
    {
        private readonly IFarmRepository farmRepository;
        private readonly BarnDbContext context;
        private readonly IMapper mapper;

        public FarmController(IFarmRepository farmRepository, BarnDbContext context, IMapper mapper)
        {
            this.farmRepository = farmRepository;
            this.context = context;
            this.mapper = mapper;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllFarms([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
         [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
          [FromQuery] int pageNumber=1, [FromQuery] int pageSize=1000)
        {
            var userId = GetUserId();
            var barnDomain = await farmRepository.GetFarmsByUserAsync(userId, filterOn,filterQuery, sortBy, isAscending ?? true, pageNumber, pageSize);

            // Convert Domain Models to DTOs
            // var regionsDto = new List<RegionDto>();
            // foreach(var region in regions)
            // {
            //     regionsDto.Add(new RegionDto()
            //     {
            //         Id = region.Id,
            //         Code = region.Code,
            //         Name = region.Name,
            //         RegionImageUrl = region.RegionImageUrl
            //     });
            // }
            var farmDto = mapper.Map<List<FarmDto>>(barnDomain);

            // Return Response
            return Ok(farmDto);
        }

        // GET SINGLE REGION by ID  
        // GET: http://localhost:5081/api/region/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var userId = GetUserId();
            var farmDomain = await farmRepository.GetFarmByIdAsync(id);

            if(farmDomain == null || farmDomain.UserId != userId)
            {
                return NotFound();
            }
            // Convert Domain Models to DTOs
            var farmDto = mapper.Map<FarmDto>(farmDomain);
            // Return DTO back to client
            return Ok(farmDto);
        }
 
        // POST CREATE REGION
        // POST: http://localhost:5081/api/region
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FarmAddDto farmAddDto)
        {   
            var userId = GetUserId();
            var farmDomainModel = mapper.Map<Farm>(farmAddDto);
            farmDomainModel.UserId = userId;

            // Add Domain Model to Database
            farmDomainModel = await farmRepository.CreateFarmAsync(farmDomainModel);

            // Convert Domain Model to DTO
            var farmDto = mapper.Map<FarmDto>(farmDomainModel);
            // Return DTO back to client
            return CreatedAtAction(nameof(GetById), new {id=farmDomainModel.Id }, farmDto);

        }
        // PUT UPDATE REGION
        // PUT: http://localhost:5081/api/region/{id}
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] FarmUpdateDto farmUpdateDto)
        {
            var userId = GetUserId();
            var existing = await farmRepository.GetFarmByIdAsync(id);
            if (existing == null || existing.UserId != userId)
            {
                return NotFound();
            }
            var toUpdate = mapper.Map<Farm>(farmUpdateDto);
            var updated = await farmRepository.UpdateFarmAsync(id, toUpdate);
            var farmDto = mapper.Map<FarmDto>(updated);
            // Return DTO back to client
            return Ok(farmDto);
            
        }

        // DELETE REGION
        // DELETE: http://localhost:5081/api/region/{id}
        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var userId = GetUserId();
            var existing = await farmRepository.GetFarmByIdAsync(id);
            if (existing == null || existing.UserId != userId)
            {
                return NotFound();
            }
            var deleted = await farmRepository.DeleteFarmAsync(id);
            var farmDto = mapper.Map<FarmDto>(deleted);
            return Ok(farmDto);
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdStr!);
        }
    }
}
