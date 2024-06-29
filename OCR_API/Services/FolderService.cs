using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OCR_API.Authorization;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.FolderTransactions;

namespace OCR_API.Services
{
    public interface IFolderService
    {
        IUnitOfWork UnitOfWork { get; }

        PageResults<FolderDto> GetAll(GetAllQuery queryParameters);

        FolderDto GetById(int folderId, PasswordDto? passwordDto = null);

        int CreateFolder(AddFolderDto folderToAdd);

        void DeleteFolder(int folderId, PasswordDto? passwordDto = null);

        void UpdateFolder(int folderId, UpdateFolderDto updateFolderDto);

        void LockFolder(int folderId, ConfirmedPasswordDto confirmedPasswordDto);

        void UnlockFolder(int folderId, PasswordDto passwordDto);
    }

    public class FolderService : IFolderService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IPasswordHasher<Folder> passwordHasher;
        private readonly IMapper mapper;
        private readonly UserActionLogger logger;
        private readonly IPaginationService queryParametersService;
        private readonly IUserContextService userContextService;

        public FolderService(IUnitOfWork unitOfWork, IPasswordHasher<Folder> passwordHasher, IMapper mapper, UserActionLogger logger,
            IPaginationService queryParametersService, IUserContextService userContextService)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.logger = logger;
            this.queryParametersService = queryParametersService;
            this.userContextService = userContextService;
        }

        public PageResults<FolderDto> GetAll(GetAllQuery queryParameters)
        {
            var userId = userContextService.GetUserId;
            var spec = new UserFoldersWithNotesSpecification(userId, queryParameters.SearchPhrase);
            var foldersQuery = UnitOfWork.Folders.GetBySpecification(spec);
            var result = queryParametersService.PreparePaginationResults<FolderDto, Folder>(queryParameters, foldersQuery, mapper);

            return result;
        }

        public FolderDto GetById(int folderId, PasswordDto? passwordDto = null)
        {
            var userId = userContextService.GetUserId;
            Folder folder = GetFolderIfBelongsToUser(userId, folderId);
            if (folder.PasswordHash is not null)
            {
                if (passwordDto is null)
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

        public int CreateFolder(AddFolderDto folderToAdd)
        {
            var userId = userContextService.GetUserId;
            AddFolderTransaction addFolderTransaction = new(UnitOfWork.Folders, passwordHasher, userId, folderToAdd.Name, folderToAdd.IconPath, folderToAdd.Password);
            addFolderTransaction.Execute();
            UnitOfWork.Commit();
            var newFolderId = addFolderTransaction.Folder.Id;
            logger.Log(EUserAction.CreateFolder, userId, DateTime.UtcNow, newFolderId);
            return newFolderId;
        }

        public void DeleteFolder(int folderId, PasswordDto? passwordDto = null)
        {
            var userId = userContextService.GetUserId;
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
            if (ResourceOperationAccess.CanEdit(folderToRemove, userId))
            {
                DeleteEntityTransaction<Folder> deleteFolderTransaction = new(UnitOfWork.Folders, folderId);
                deleteFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.DeleteFolder, userId, DateTime.UtcNow, folderId);
            }
            else
            {
                throw new ForbidException("Cannot operate someone else's folder.");
            }
        }

        public void UpdateFolder(int folderId, UpdateFolderDto updateFolderDto)
        {
            var userId = userContextService.GetUserId;
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
            if (ResourceOperationAccess.CanEdit(folderToUpdate, userId))
            {
                UpdateFolderTransaction updateFolderTransaction = new(folderToUpdate, updateFolderDto.Name, updateFolderDto.IconPath);
                updateFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.UpdateFolder, userId, DateTime.UtcNow, folderId);
            }
            else
            {
                throw new ForbidException("Cannot operate someone else's folder.");
            }
        }

        public void LockFolder(int folderId, ConfirmedPasswordDto confirmedPasswordDto)
        {
            var userId = userContextService.GetUserId;
            Folder folderToLock = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToLock.PasswordHash is not null)
            {
                throw new BadRequestException("The folder is already locked.");
            }
            if (ResourceOperationAccess.CanEdit(folderToLock, userId))
            {
                LockFolderTransaction lockFolderTransaction = new(folderToLock, passwordHasher, confirmedPasswordDto.Password);
                lockFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.LockFolder, userId, DateTime.UtcNow, folderId);
            }
            else
            {
                throw new ForbidException("Cannot operate someone else's folder.");
            }
        }

        public void UnlockFolder(int folderId, PasswordDto passwordDto)
        {
            var userId = userContextService.GetUserId;
            Folder folderToUnlock = GetFolderIfBelongsToUser(userId, folderId);
            if (folderToUnlock.PasswordHash is null)
            {
                throw new BadRequestException("The folder is already unlocked.");
            }
            var result = passwordHasher.VerifyHashedPassword(folderToUnlock, folderToUnlock.PasswordHash, passwordDto.Password);
            if (result != PasswordVerificationResult.Success)
            {
                throw new BadRequestException("Invalid password.");
            }
            if (ResourceOperationAccess.CanEdit(folderToUnlock, userId))
            {
                UnlockFolderTransaction unlockFolderTransaction = new(folderToUnlock);
                unlockFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.UnlockFolder, userId, DateTime.UtcNow, folderId);
            }
            else
            {
                throw new ForbidException("Cannot operate someone else's folder.");
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
            if (folder.UserId != userId && !ResourceOperationAccess.IsShared(folder, userId))
            {
                throw new ForbidException("Cannot operate someone else's folder.");
            }
            return folder;
        }
    }
}