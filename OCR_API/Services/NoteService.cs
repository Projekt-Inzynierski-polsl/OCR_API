using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;
using OCR_API.ModelsDto;

namespace OCR_API.Services
{
    public interface INoteService
    {
        IUnitOfWork UnitOfWork { get; }

    }
    public class NoteService : INoteService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;

        public NoteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
        }
    }
}
