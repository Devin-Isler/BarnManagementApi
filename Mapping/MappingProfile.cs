using AutoMapper;
using BarnManagementApi.Models.Domain;
using BarnManagementApi.Models.DTO;

namespace BarnManagementApi.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Farm, FarmDto>().ReverseMap();
            CreateMap<FarmAddDto, Farm>().ReverseMap();
            CreateMap<FarmUpdateDto, Farm>().ReverseMap();

            CreateMap<Animal, AnimalDto>().ReverseMap();
            CreateMap<AnimalBuyDto, Animal>().ReverseMap();
            CreateMap<AnimalUpdateDto, Animal>().ReverseMap();

            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<ProductAddDto, Product>().ReverseMap();
            CreateMap<ProductUpdateDto, Product>().ReverseMap();

            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
