using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.ModelsDto;
using OCR_API.Specifications;
using OCR_API.Transactions.FolderTransactions;
using OCR_API.Transactions.UserTransactions;

namespace OCR_API.Services
{
    public interface IFolderService
    {
        IUnitOfWork UnitOfWork { get; }
        IEnumerable<FolderDto> GetAll(string jwtToken);
        FolderDto GetById(int id, PasswordDto? passwordDto);
        int CreateFolder(string jwtToken, AddFolderDto folderToAdd);
        void DeleteFolder(int folderId);
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
            var spec = new UserFoldersSpecification(userId);
            var folders = UnitOfWork.Folders.GetBySpecification(spec);
            var foldersDto = folders.Select(f => mapper.Map<FolderDto>(f)).ToList();

            return foldersDto;
        }
        public FolderDto GetById(int folderId, PasswordDto? passwordDto)
        {
            var spec = new FolderByIdWithNotesSpecification(folderId);
            var folder = UnitOfWork.Folders.GetBySpecification(spec).FirstOrDefault();
            if (folder.PasswordHash != string.Empty)
            {
                var result = passwordHasher.VerifyHashedPassword(folder, folder.PasswordHash, passwordDto.Password);
                if (result != PasswordVerificationResult.Success)
                {
                    throw new BadRequestException("Invalid password.");
                }
            }
            var folderDto = mapper.Map<FolderDto>(folder);

            return folderDto;
        }

        public int CreateFolder(string jwtToken, AddFolderDto folderToAdd)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            AddFolderTransaction addFolderTransaction = new(UnitOfWork.Folders, passwordHasher, userId, folderToAdd.Name, folderToAdd.IconPath, folderToAdd.Password);
            addFolderTransaction.Execute();
            UnitOfWork.Commit();
            var newFolderId = addFolderTransaction.Folder.Id;
            return newFolderId;
        }

        public void DeleteFolder(int folderId)
        {
            DeleteFolderTransaction deleteFolderTransaction = new(UnitOfWork.Folders, folderId);
            deleteFolderTransaction.Execute();
            UnitOfWork.Commit();
        }
    }
}