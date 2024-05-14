using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto.UploadedModelDtos;
using OCR_API.ModelsDto.UserLogDtos;

namespace OCR_API.MappingProfiles
{
    public class UserLogMappingProfile : Profile
    {
        public UserLogMappingProfile()
        {
            CreateMap<UserLogDto, UserLog>();
            CreateMap<UserLog, UserLogDto>();
        }
    }
}
