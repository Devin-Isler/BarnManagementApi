// Mapping Profile - Defines the mapping between domain and DTO objects
// Used for converting between different representations of the same data

using AutoMapper;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;

namespace BarnManagementApi.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Farm mappings
            CreateMap<Farm, FarmDto>().ReverseMap();
            CreateMap<FarmAddDto, Farm>().ReverseMap();
            CreateMap<FarmUpdateDto, Farm>().ReverseMap();

            // Animal mappings
            CreateMap<Animal, AnimalDto>().ReverseMap();
            CreateMap<AnimalBuyDto, Animal>().ReverseMap();
            CreateMap<AnimalUpdateDto, Animal>().ReverseMap();

            // Product mappings
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<ProductAddDto, Product>().ReverseMap();
            CreateMap<ProductUpdateDto, Product>().ReverseMap();
            
            // User mappings
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
