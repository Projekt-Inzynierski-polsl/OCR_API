using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.UploadedModelDtos;

namespace OCR_API.MappingProfiles
{
    public class NoteWordErrorMappingProfile : Profile
    {
        public NoteWordErrorMappingProfile()
        {
            CreateMap<NoteWordErrorDto, NoteWordError>();
            CreateMap<NoteWordError, NoteWordErrorDto>();
        }
    }
}
