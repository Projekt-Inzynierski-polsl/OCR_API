namespace OCR_API.ModelsDto.UploadedModelDtos
{
    public class UploadedModelDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int UserId { get; set; }
        public DateTime UploadTime { get; set; }
    }
}
