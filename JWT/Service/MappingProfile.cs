using System;
using AutoMapper;
using Entity.ModelsDtos;
using JWT.Core.Model;
using JWT.Model;

namespace Business
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CustomUser, UserDto>().ReverseMap();
            CreateMap<Customer, CustomerDto>().ReverseMap(); ;
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<CustomUser, UserForRegisterDto>().ReverseMap();
        }
    }
}
