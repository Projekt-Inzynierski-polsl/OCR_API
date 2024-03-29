using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto;

namespace OCR_API.MappingProfiles
{
    public class FolderMappingProfile : Profile
    {
        public FolderMappingProfile()
        {
            CreateMap<Folder, FolderDto>()
             .ForMember(dest => dest.hasPassword, opt =>
             {
                 opt.MapFrom(src => !string.IsNullOrEmpty(src.PasswordHash));
             });
        }
    }
}
