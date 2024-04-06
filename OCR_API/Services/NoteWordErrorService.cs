using AutoMapper;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.UploadedModelDtos;
using OCR_API.Specifications;

namespace OCR_API.Services
{

    public interface INoteWordErrorService
    {
        public IUnitOfWork UnitOfWork { get; }
        IEnumerable<NoteWordErrorDto> GetAll();
        IEnumerable<NoteWordErrorDto> GetAllForUser(int userId);
        NoteWordErrorDto GetById(int errorId);
        void DeleteById(int errorId);
        void DeleteAll();
        MemoryStream DownloadErrors();
    }

    public class NoteWordErrorService : INoteWordErrorService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;

        public NoteWordErrorService(IUnitOfWork unitOfWork, IMapper mapper, JwtTokenHelper jwtTokenHelper)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
        }

        public IEnumerable<NoteWordErrorDto> GetAll()
        {
            var errors = UnitOfWork.NoteWordErrors.GetAll().Select(f => mapper.Map<NoteWordErrorDto>(f)).ToList();
            return errors;
        }
        public IEnumerable<NoteWordErrorDto> GetAllForUser(int userId)
        {
            var error = UnitOfWork.NoteWordErrors.GetAllByUser(userId).Select(f => mapper.Map<NoteWordErrorDto>(f)).ToList();
            return error;
        }
        public NoteWordErrorDto GetById(int errorId)
        {
            var error = UnitOfWork.NoteWordErrors.GetById(errorId);
            var errorDto = mapper.Map<NoteWordErrorDto>(error);

            return errorDto;

        }

        public void DeleteById(int errorId)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll()
        {
            throw new NotImplementedException();
        }

        public MemoryStream DownloadErrors()
        {
            throw new NotImplementedException();
        }
    }
   
}
