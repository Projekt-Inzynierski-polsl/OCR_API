using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.UploadedModelDtos;

namespace OCR_API.MappingProfiles
{
    public class UploadedModelMappingProfile : Profile
    {
        public UploadedModelMappingProfile()
        {
            CreateMap<UploadedModelDto, UploadedModel>();
            CreateMap<UploadedModel, UploadedModelDto>();
        }
    }
}
