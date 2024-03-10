using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using System.Security.Cryptography;

namespace OCR_API.MappingProfiles
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile() 
        {
            CreateMap<RegisterUserDto, User>();
        }
    }
}
