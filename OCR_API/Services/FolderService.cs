using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog.Filters;
using OCR_API.Entities;
using OCR_API.Enums;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.FolderTransactions;
using OCR_API.Transactions.UserTransactions;

namespace OCR_API.Services
{
    public interface IFolderService
    {
        IUnitOfWork UnitOfWork { get; }
        IEnumerable<FolderDto> GetAll(string jwtToken, string? searchPhrase = null);
        FolderDto GetById(string jwtToken, int id, PasswordDto? passwordDto = null);
        int CreateFolder(string jwtToken, AddFolderDto folderToAdd);
        void DeleteFolder(string jwtToken, int folderId, PasswordDto passwordDto = null);
        void UpdateFolder(string jwtToken, int folderId, UpdateFolderDto updateFolderDto);
        void LockFolder(string jwtToken, int folderId, ConfirmedPasswordDto confirmedPasswordDto);
        void UnlockFolder(string jwtToken, int folderId, PasswordDto passwordDto);
    }
    public class FolderService : IFolderService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IPasswordHasher<Folder> passwordHasher;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly UserActionLogger logger;

        public FolderService(IUnitOfWork unitOfWork, IPasswordHasher<Folder> passwordHasher, IMapper mapper, JwtTokenHelper jwtTokenHelper, UserActionLogger logger)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
            this.logger = logger;
        }

        public IEnumerable<FolderDto> GetAll(string jwtToken, string? searchPhrase = null)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var spec = new UserFoldersWithNotesSpecification(userId, searchPhrase);
            var folders = UnitOfWork.Folders.GetBySpecification(spec);
            var foldersDto = folders.Select(f => mapper.Map<FolderDto>(f)).ToList();

            return foldersDto;
        }
        public FolderDto GetById(string jwtToken, int folderId, PasswordDto passwordDto = null)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folder = GetFolderIfBelongsToUser(userId, folderId);
            if (folder.PasswordHash is not null)
            {
                if(passwordDto is null)
                {
                    throw new BadRequestException("Folder is locked.");
                }
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
            logger.Log(EUserAction.CreateFolder, userId, DateTime.UtcNow, newFolderId);
            return newFolderId;
        }

        public void DeleteFolder(string jwtToken, int folderId, PasswordDto passwordDto = null)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folderToRemove = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToRemove.PasswordHash is not null)
            {
                if (passwordDto is null)
                {
                    throw new BadRequestException("Folder is locked.");
                }
                var result = passwordHasher.VerifyHashedPassword(folderToRemove, folderToRemove.PasswordHash, passwordDto.Password);
                if (result != PasswordVerificationResult.Success)
                {
                    throw new BadRequestException("Invalid password.");
                }
            }
            if (CanEdit(folderToRemove, userId))
            {
                DeleteEntityTransaction<Folder> deleteFolderTransaction = new(UnitOfWork.Folders, folderId);
                deleteFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.DeleteFolder, userId, DateTime.UtcNow, folderId);
            }
            else
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's folder.");
            }
        }

        public void UpdateFolder(string jwtToken, int folderId, UpdateFolderDto updateFolderDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folderToUpdate = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToUpdate.PasswordHash is not null)
            {
                if (updateFolderDto.PasswordToFolder is null)
                {
                    throw new BadRequestException("Folder is locked.");
                }
                var result = passwordHasher.VerifyHashedPassword(folderToUpdate, folderToUpdate.PasswordHash, updateFolderDto.PasswordToFolder);
                if (result != PasswordVerificationResult.Success)
                {
                    throw new BadRequestException("Invalid password.");
                }
            }
            if (CanEdit(folderToUpdate, userId))
            {
                UpdateFolderTransaction updateFolderTransaction = new(folderToUpdate, updateFolderDto.Name, updateFolderDto.IconPath);
                updateFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.UpdateFolder, userId, DateTime.UtcNow, folderId);
            }
            else
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's folder.");
            }
        }

        public void LockFolder(string jwtToken, int folderId, ConfirmedPasswordDto confirmedPasswordDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folderToLock = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToLock.PasswordHash is not null)
            {
                throw new BadRequestException("The folder is already locked.");
            }
            if (CanEdit(folderToLock, userId))
            {
                LockFolderTransaction lockFolderTransaction = new(folderToLock, passwordHasher, confirmedPasswordDto.Password);
                lockFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.LockFolder, userId, DateTime.UtcNow, folderId);
            }
            else
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's folder.");
            }
        }

        public void UnlockFolder(string jwtToken, int folderId, PasswordDto passwordDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Folder folderToUnlock = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToUnlock.PasswordHash is null)
            {
                throw new BadRequestException("The folder is already unlocked.");
            }
            var result = passwordHasher.VerifyHashedPassword(folderToUnlock, folderToUnlock.PasswordHash, passwordDto.Password);
            if(result != PasswordVerificationResult.Success)
            {
                throw new BadRequestException("Invalid password.");
            }
            if(CanEdit(folderToUnlock, userId))
            {
                UnlockFolderTransaction unlockFolderTransaction = new(folderToUnlock);
                unlockFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.UnlockFolder, userId, DateTime.UtcNow, folderId);
            }
            else
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's folder.");
            }

        }

        private Folder GetFolderIfBelongsToUser(int userId, int folderId)
        {
            var spec = new FolderByIdWithNotesAndSharedSpecification(folderId, userId);
            var folder = UnitOfWork.Folders.GetBySpecification(spec).FirstOrDefault();
            if (folder is null)
            {
                throw new NotFoundException("That entity doesn't exist.");
            }
            if (folder.UserId != userId && !IsShared(folder, userId))
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's folder.");
            }
            return folder;
        }

        private bool IsShared(Folder folder, int userId)
        {
            return folder.SharedObjects.Where(f => f.UserId == userId).Count() > 0 || folder.SharedObjects.Where(f => f.UserId == null).Count() > 0;
        }

        private bool CanEdit(Folder folder, int userId)
        {
            return folder.UserId == userId || folder.SharedObjects.FirstOrDefault(f => f.UserId == null).ModeId == (int)EShareMode.Edit || folder.SharedObjects.FirstOrDefault(f => f.UserId == userId).ModeId == (int)EShareMode.Edit;
        }
    }
}