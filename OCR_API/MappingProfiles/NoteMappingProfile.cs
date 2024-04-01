using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto;

namespace OCR_API.MappingProfiles
{
    public class NoteMappingProfile : Profile
    {
        public NoteMappingProfile()
        {
            CreateMap<Note, NoteDto>();
            CreateMap<AddNoteDto, Note>()
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.CategoriesIds.Select(id => new NoteCategory { Id = id })));
        }
    }
}
