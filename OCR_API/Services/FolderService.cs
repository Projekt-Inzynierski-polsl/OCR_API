using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog.Filters;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Specifications;
using OCR_API.Transactions.FolderTransactions;
using OCR_API.Transactions.UserTransactions;

namespace OCR_API.Services
{
    public interface IFolderService
    {
        IUnitOfWork UnitOfWork { get; }
        IEnumerable<FolderDto> GetAll(string jwtToken);
        FolderDto GetById(string jwtToken, int id, PasswordDto? passwordDto = null);
        int CreateFolder(string jwtToken, AddFolderDto folderToAdd);
        void DeleteFolder(string jwtToken, int folderId, PasswordDto passwordDto = null);
        void UpdateFolder(string jwtToken, int folderId, UpdateFolderDto updateFolderDto, PasswordDto passwordDto = null);
        void LockFolder(string jwtToken, int folderId, ConfirmedPasswordDto confirmedPasswordDto);
        void UnlockFolder(string jwtToken, int folderId, PasswordDto passwordDto);
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
        public FolderDto GetById(string jwtToken, int folderId, PasswordDto? passwordDto = null)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folder = GetFolderIfBelongsToUser(userId, folderId);
            if (folder.UserId != userId)
            {
                throw new UnauthorizedAccessException("Cannot access to someone else's folder.");
            }
            if (folder.PasswordHash is not null)
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

        public void DeleteFolder(string jwtToken, int folderId, PasswordDto passwordDto = null)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folderToRemove = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToRemove.PasswordHash is not null)
            {
                var result = passwordHasher.VerifyHashedPassword(folderToRemove, folderToRemove.PasswordHash, passwordDto.Password);
                if (result != PasswordVerificationResult.Success)
                {
                    throw new BadRequestException("Invalid password.");
                }
            }
            DeleteFolderTransaction deleteFolderTransaction = new(UnitOfWork.Folders, folderId);
            deleteFolderTransaction.Execute();
            UnitOfWork.Commit();
        }

        public void UpdateFolder(string jwtToken, int folderId, UpdateFolderDto updateFolderDto, PasswordDto passwordDto = null)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folderToUpdate = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToUpdate.UserId != userId)
            {
                throw new UnauthorizedAccessException("Cannot delete someone else's folder.");
            }
            UpdateFolderTransaction updateFolderTransaction = new(folderToUpdate, updateFolderDto.Name, updateFolderDto.IconPath);
            updateFolderTransaction.Execute();
            UnitOfWork.Commit();
        }

        public void LockFolder(string jwtToken, int folderId, ConfirmedPasswordDto confirmedPasswordDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folderToLock = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToLock.PasswordHash is not null)
            {
                throw new UnauthorizedAccessException("The folder is already locked.");
            }
            LockFolderTransaction lockFolderTransaction = new(folderToLock, passwordHasher, confirmedPasswordDto.Password);
            lockFolderTransaction.Execute();
            UnitOfWork.Commit();

        }

        public void UnlockFolder(string jwtToken, int folderId, PasswordDto passwordDto)
        {

            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folderToUnlock = UnitOfWork.Folders.GetById(folderId);
            var result = passwordHasher.VerifyHashedPassword(folderToUnlock, folderToUnlock.PasswordHash, passwordDto.Password);
            if(result == PasswordVerificationResult.Success)
            {
                throw new BadRequestException("Invalid password.");
            }
            if (folderToUnlock.UserId != userId)
            {
                throw new UnauthorizedAccessException("Cannot unlock someone else's folder.");
            }
            if (folderToUnlock.PasswordHash is null)
            {
                throw new UnauthorizedAccessException("The folder is already unlocked.");
            }
            UnlockFolderTransaction unlockFolderTransaction = new(folderToUnlock);
            unlockFolderTransaction.Execute();
            UnitOfWork.Commit();
        }

        private Folder GetFolderIfBelongsToUser(int userId, int folderId)
        {
            var spec = new FolderByIdWithNotesSpecification(folderId);
            var folder = UnitOfWork.Folders.GetBySpecification(spec).FirstOrDefault();
            if (folder.UserId != userId)
            {
                throw new UnauthorizedAccessException("Cannot lock someone else's folder.");
            }
            return folder;
        }
    }
}