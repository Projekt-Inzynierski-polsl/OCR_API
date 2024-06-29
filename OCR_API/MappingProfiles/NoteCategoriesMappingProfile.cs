using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto.NoteCategoriesDtos;

namespace OCR_API.MappingProfiles
{
    public class NoteCategoriesMappingProfile : Profile
    {
        public NoteCategoriesMappingProfile()
        {
            CreateMap<NoteCategory, NoteCategoryDto>();
            CreateMap<NoteCategoryDto, NoteCategory>();
            CreateMap<NoteCategory, ActionNoteCategoryDto>();
            CreateMap<ActionNoteCategoryDto, NoteCategory>();
        }
    }
}