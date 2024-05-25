using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto.SharedDtos;

namespace OCR_API.MappingProfiles
{
    public class SharedMappingProfile : Profile
    {
        public SharedMappingProfile()
        {
            CreateMap<Shared, SharedDto>()
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.Folder, opt => opt.MapFrom(src => src.Folder));
        }
    }
}
