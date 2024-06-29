using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.BoundingBoxDtos;
using OCR_API.ModelsDto.NoteFileDtos;
using OCR_API.ModelsDto.NoteLineDtos;

namespace OCR_API.MappingProfiles
{
    public class NoteFileMappingProfile : Profile
    {
        public NoteFileMappingProfile()
        {
            CreateMap<NoteFile, NoteFileDto>()
                .ForMember(dest => dest.BoundingBoxes, opt => opt.MapFrom(src => src.BoundingBoxes));

            CreateMap<BoundingBoxDto, BoundingBox>();
            CreateMap<BoundingBox, BoundingBoxDto>()
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines));

            CreateMap<NoteLine, NoteLineDto>();
            CreateMap<NoteLineDto, NoteLine>();

            CreateMap<NoteWordError, NoteWordErrorDto>();
            CreateMap<NoteWordErrorDto, NoteWordError>();
            CreateMap<AddErrorDto, NoteWordError>();
            CreateMap<NoteWordError, AddErrorDto>();
        }
    }
}