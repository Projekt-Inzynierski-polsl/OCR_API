using AutoMapper;
using Azure.Core;
using OCR_API.Exceptions;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.UploadedModelDtos;
using OCR_API.Specifications;
using OCR_API.Transactions;

namespace OCR_API.Services
{
    public interface IUploadedModelService
    {
        IUnitOfWork UnitOfWork { get; }

        IEnumerable<UploadedModelDto> GetAll();
        IEnumerable<UploadedModelDto> GetAllByUserId(int userId);
        UploadedModelDto GetById(int modelId);
        Task UploadNewModelAsync(string accessToken, IFormFile modelToUpload);
    }
    public class UploadedModelService : IUploadedModelService
    {
        private const string UPLOADED_MODEL_DICTIONARY_PATH = "uploaded_models";
        private const string FILE_EXTENSION = ".pt";
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;

        public UploadedModelService(IUnitOfWork unitOfWork, IMapper mapper, JwtTokenHelper jwtTokenHelper)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
        }

        public IEnumerable<UploadedModelDto> GetAll()
        {
            var models = UnitOfWork.UploadedModels.GetAll().Select(f => mapper.Map<UploadedModelDto>(f)).ToList();
            return models;
        }

        public IEnumerable<UploadedModelDto> GetAllByUserId(int userId)
        {
            var spec = new UploadedModelsByUserIdSpecification(userId);
            var models = UnitOfWork.UploadedModels.GetBySpecification(spec).Select(f => mapper.Map<UploadedModelDto>(f)).ToList();
            return models;
        }

        public UploadedModelDto GetById(int modelId)
        {
            var model = mapper.Map<UploadedModelDto>(UnitOfWork.UploadedModels.GetById(modelId));
            return model;
        }

        public async Task UploadNewModelAsync(string jwtToken, IFormFile modelToUpload)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);

            if (modelToUpload.Length > 0)
            {
                var fileName = Path.GetFileName(modelToUpload.FileName);

                var fileExtension = Path.GetExtension(fileName);
                if (fileExtension != FILE_EXTENSION)
                {
                    throw new BadRequestException("Wrong file extension");
                }
                UploadModelTransaction uploadModelTransaction = new UploadModelTransaction(UnitOfWork.UploadedModels, userId, UPLOADED_MODEL_DICTIONARY_PATH);
                uploadModelTransaction.Execute();
                UnitOfWork.Commit();
                if (!Directory.Exists(UPLOADED_MODEL_DICTIONARY_PATH))
                {
                    Directory.CreateDirectory(UPLOADED_MODEL_DICTIONARY_PATH);
                }
                var filePath = Path.Combine(UPLOADED_MODEL_DICTIONARY_PATH, uploadModelTransaction.model.Id.ToString() + FILE_EXTENSION);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await modelToUpload.CopyToAsync(stream);
                }
            }
            else
            {
                throw new BadRequestException("Empty file.");
            }

        }
    }
}
