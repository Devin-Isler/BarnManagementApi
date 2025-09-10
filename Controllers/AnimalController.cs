using AutoMapper;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;
using BarnManagementApi.Repository;
using BarnManagementApi.Services;
using BarnManagementApi.Services.Interfaces;
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
        private readonly IAnimalService animalService;

        public AnimalController(IAnimalRepository animalRepository, IMapper mapper, IAnimalService animalService)
        {
            this.animalRepository = animalRepository;
            this.mapper = mapper;
            this.animalService = animalService;
        }

        [HttpGet]
        [Authorize(Roles = "Writer, Reader")]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var domain = await animalRepository.GetAllAnimalsAsync();
            domain = domain.Where(a => a.Farm != null && a.Farm.UserId == userId).ToList();
            var dto = mapper.Map<List<AnimalDto>>(domain);
            return Ok(dto);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer, Reader")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var userId = GetUserId();
            var domain = await animalRepository.GetAnimalByIdAsync(id);
            if (domain == null || domain.Farm == null || domain.Farm.UserId != userId) return NotFound();
            var dto = mapper.Map<AnimalDto>(domain);
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Create([FromBody] AnimalAddDto addDto)
        {
            var userId = GetUserId();
            var domain = mapper.Map<Animal>(addDto);
            domain = await animalRepository.CreateAnimalAsync(domain);
            var dto = mapper.Map<AnimalDto>(domain);
            return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
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

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var userId = GetUserId();
            var existing = await animalRepository.GetAnimalByIdAsync(id);
            if (existing == null || existing.Farm == null || existing.Farm.UserId != userId) return NotFound();
            var domain = await animalRepository.DeleteAnimalAsync(id);
            var dto = mapper.Map<AnimalDto>(domain);
            return Ok(dto);
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdStr!);
        }
    }
}
