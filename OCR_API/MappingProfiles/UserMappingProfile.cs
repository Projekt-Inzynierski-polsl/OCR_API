using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto;

namespace OCR_API.MappingProfiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile() 
        {
            CreateMap<RegisterUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<User, UserDto>();
        }
    }
}
