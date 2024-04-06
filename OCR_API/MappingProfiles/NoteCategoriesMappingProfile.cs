using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto.NoteCategoriesDtos;
using OCR_API.ModelsDto.UploadedModelDtos;

namespace OCR_API.MappingProfiles
{
    public class NoteCategoriesMappingProfile : Profile
    {
        public NoteCategoriesMappingProfile()
        {
            CreateMap<NoteCategory, NoteCategoryDto>();
            CreateMap<NoteCategoryDto, NoteCategory>();
            CreateMap<NoteCategory, NameNoteCategoryDto>();
            CreateMap<NameNoteCategoryDto, NoteCategory>();
        }
    }
}
