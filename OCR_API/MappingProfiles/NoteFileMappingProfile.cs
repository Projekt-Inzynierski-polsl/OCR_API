using AutoMapper;
using Newtonsoft.Json;
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
                .ForMember(dest => dest.BoundingBoxes, opt => opt.MapFrom(src => src.BoundingBoxes
                .Select(b => new BoundingBoxDto
                {
                    Coordinates = JsonConvert.DeserializeObject<Coords>(b.Coordinates),
                    Lines = b.Lines
                .Select(l => new NoteLineDto { Content = l.Content, Coordinates = JsonConvert.DeserializeObject<Coords>(l.Coordinates) }).ToList()
                })));

            CreateMap<NoteFileDto, NoteFile>();

            CreateMap<BoundingBoxDto, BoundingBox>();
            CreateMap<BoundingBox, BoundingBoxDto>();

            CreateMap<NoteLine, NoteLineDto>();
            CreateMap<NoteLineDto, NoteLine>();
        }
    }
}
