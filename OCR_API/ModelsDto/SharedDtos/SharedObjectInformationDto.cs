using OCR_API.Enums;

namespace OCR_API.ModelsDto.SharedDtos
{
    public class SharedObjectInformationDto
    {
        public int ObjectId { get; set; }
        public string ShareToEmail { get; set; }
        public EShareMode Mode { get; set; }
    }
}
