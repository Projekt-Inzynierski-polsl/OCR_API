using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Specifications;
using OCR_API.Transactions.UserTransactions;

namespace OCR_API.Services
{
    public interface IFolderService
    {
        IUnitOfWork UnitOfWork { get; }
        IEnumerable<FolderDto> GetAll(string jwtToken);
        FolderDto GetById(int id);
    }
    public class FolderService : IFolderService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IPasswordHasher<Folder> passwordHasher;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;

        public FolderService(IUnitOfWork unitOfWork, IPasswordHasher<Folder> passwordHasher, IMapper mapper, JwtTokenHelper jwtTokenHelper)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
        }

        public IEnumerable<FolderDto> GetAll(string jwtToken)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var spec = new UserFoldersWithNotesSpecification(userId);
            var folders = UnitOfWork.Folders.GetBySpecification(spec);
            var foldersDto = folders.Select(f => mapper.Map<FolderDto>(f)).ToList();

            return foldersDto;
        }
        public FolderDto GetById(int folderId)
        {
            var spec = new UserFoldersWithNotesSpecification(folderId);
            var folder = UnitOfWork.Folders.GetBySpecification(spec).FirstOrDefault();
            var folderDto = mapper.Map<FolderDto>(folder);

            return folderDto;
        }

    }
}