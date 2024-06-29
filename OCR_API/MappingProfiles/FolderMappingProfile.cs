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
             .ForMember(dest => dest.HasPassword, opt =>
             {
                 opt.MapFrom(src => !string.IsNullOrEmpty(src.PasswordHash));
             })
              .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
              .ForMember(dest => dest.SharedObjects, opt => opt.MapFrom(src => src.SharedObjects));

            CreateMap<Shared, FolderDto>()
                             .ForMember(dest => dest.HasPassword, opt =>
                             {
                                 opt.MapFrom(src => !string.IsNullOrEmpty(src.Folder.PasswordHash));
                             }); ;
        }
    }
}