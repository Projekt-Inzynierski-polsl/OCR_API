using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;
using OCR_API.Enums;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto.SharedDtos;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.FolderTransactions;
using OCR_API.Transactions.SharedTransactions;

namespace OCR_API.Services
{
    public interface ISharedService
    {
        IUnitOfWork UnitOfWork { get; }

        IEnumerable<SharedDto> GetAllFoldersByUserId();

        IEnumerable<SharedDto> GetAllNotesByUserId();

        IEnumerable<SharedObjectInformationDto> GetInformationAboutSharedFolder(int folderId);

        IEnumerable<SharedObjectInformationDto> GetInformationAboutSharedNote(int noteId);

        void ShareFolder(SharedObjectDto sharedObjectDto);

        void ShareNote(SharedObjectDto sharedObjectDto);

        void UnshareFolder(SharedObjectDto sharedObjectDto);

        void UnshareNote(SharedObjectDto sharedObjectDto);
    }

    public class SharedService : ISharedService
    {
        public IUnitOfWork UnitOfWork { get; }

        private readonly IPasswordHasher<Folder> passwordHasher;
        private readonly IMapper mapper;
        private readonly UserActionLogger logger;
        private readonly IUserContextService userContextService;

        public SharedService(IUnitOfWork unitOfWork, IPasswordHasher<Folder> passwordHasher, IMapper mapper, UserActionLogger logger,
            IUserContextService userContextService)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.logger = logger;
            this.userContextService = userContextService;
        }

        public IEnumerable<SharedDto> GetAllFoldersByUserId()
        {
            var userId = userContextService.GetUserId;

            var spec = new SharedFoldersWithNotesSpecification(userId);
            var shares = UnitOfWork.Shared.GetBySpecification(spec);
            var sharesDto = shares.Select(mapper.Map<SharedDto>).ToList();

            return sharesDto;
        }

        public IEnumerable<SharedDto> GetAllNotesByUserId()
        {
            var userId = userContextService.GetUserId;

            var spec = new SharedNotesWithFileAndCategoriesSpecification(userId);
            var shares = UnitOfWork.Shared.GetBySpecification(spec);
            var sharesDto = shares.Select(mapper.Map<SharedDto>).ToList();

            return sharesDto;
        }

        public IEnumerable<SharedObjectInformationDto> GetInformationAboutSharedFolder(int folderId)
        {
            var userId = userContextService.GetUserId;
            var folder = GetFolderIfBelongsToUser(userId, folderId);

            var spec = new SharesByFolderIdSpecification(folderId);
            var shares = UnitOfWork.Shared.GetBySpecification(spec);

            var sharesInformationDto = shares.Select(f => new SharedObjectInformationDto() { ObjectId = folderId, ShareToEmail = f.User.Email, Mode = (EShareMode)f.Mode.Id }).ToList();
            return sharesInformationDto;
        }

        public IEnumerable<SharedObjectInformationDto> GetInformationAboutSharedNote(int noteId)
        {
            var userId = userContextService.GetUserId;
            var note = GetNoteIfBelongsToUser(userId, noteId);

            var spec = new SharesByNoteIdSpecification(noteId);
            var shares = UnitOfWork.Shared.GetBySpecification(spec);

            var sharesInformationDto = shares.Select(f => new SharedObjectInformationDto() { ObjectId = noteId, ShareToEmail = f.User.Email, Mode = (EShareMode)f.Mode.Id }).ToList();
            return sharesInformationDto;
        }

        public void ShareFolder(SharedObjectDto sharedObjectDto)
        {
            GetUserIdAndShareUserId(sharedObjectDto, out int userId, out int? shareUserId);
            Folder folder = GetFolderIfBelongsToUser(userId, sharedObjectDto.ObjectId);
            if (folder.PasswordHash is not null)
            {
                if (sharedObjectDto.Password is null)
                {
                    throw new BadRequestException("Folder is locked.");
                }
                var result = passwordHasher.VerifyHashedPassword(folder, folder.PasswordHash, sharedObjectDto.Password);
                if (result != PasswordVerificationResult.Success)
                {
                    throw new BadRequestException("Invalid password.");
                }
            }
            bool isAlreadyShared = (shareUserId != null && UnitOfWork.Shared.GetAllByUser(shareUserId.Value).Any(f => f.FolderId == sharedObjectDto.ObjectId))
                || UnitOfWork.Shared.GetAll().Any(f => f.FolderId == sharedObjectDto.ObjectId && f.UserId is null);
            if (isAlreadyShared)
            {
                throw new BadRequestException("That object is already shared.");
            }
            ShareTransaction shareFolderTransaction = new ShareFolderTransaction(UnitOfWork.Shared, shareUserId, sharedObjectDto.ObjectId, sharedObjectDto.ShareMode);
            shareFolderTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.ShareFolder, userId, DateTime.UtcNow, sharedObjectDto.ObjectId);
        }

        public void ShareNote(SharedObjectDto sharedObjectDto)
        {
            GetUserIdAndShareUserId(sharedObjectDto, out int userId, out int? shareUserId);
            Note Note = GetNoteIfBelongsToUser(userId, sharedObjectDto.ObjectId);
            bool isAlreadyShared = (shareUserId != null && UnitOfWork.Shared.GetAllByUser(shareUserId.Value).Any(f => f.NoteId == sharedObjectDto.ObjectId))
                || UnitOfWork.Shared.GetAll().Any(f => f.NoteId == sharedObjectDto.ObjectId && f.UserId is null);
            if (isAlreadyShared)
            {
                throw new BadRequestException("That object is already shared.");
            }
            ShareTransaction shareNoteTransaction = new ShareNoteTransaction(UnitOfWork.Shared, shareUserId, sharedObjectDto.ObjectId, sharedObjectDto.ShareMode);
            shareNoteTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.ShareNote, userId, DateTime.UtcNow, sharedObjectDto.ObjectId);
        }

        public void UnshareFolder(SharedObjectDto sharedObjectDto)
        {
            GetUserIdAndShareUserId(sharedObjectDto, out int userId, out int? shareUserId);
            Folder folder = GetFolderIfBelongsToUser(userId, sharedObjectDto.ObjectId);
            if (folder.PasswordHash is not null)
            {
                if (sharedObjectDto.Password is null)
                {
                    throw new BadRequestException("Folder is locked.");
                }
                var result = passwordHasher.VerifyHashedPassword(folder, folder.PasswordHash, sharedObjectDto.Password);
                if (result != PasswordVerificationResult.Success)
                {
                    throw new BadRequestException("Invalid password.");
                }
            }
            var entityToDelete = UnitOfWork.Shared.Entity.FirstOrDefault(e => e.UserId == shareUserId && e.FolderId == sharedObjectDto.ObjectId);
            if (entityToDelete is null)
            {
                throw new BadRequestException("That entity doesn't exist.");
            }
            DeleteEntityTransaction<Shared> shareFolderTransaction = new(UnitOfWork.Shared, entityToDelete.Id);
            shareFolderTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.UnshareFolder, userId, DateTime.UtcNow, sharedObjectDto.ObjectId);
        }

        public void UnshareNote(SharedObjectDto sharedObjectDto)
        {
            GetUserIdAndShareUserId(sharedObjectDto, out int userId, out int? shareUserId);
            Note folder = GetNoteIfBelongsToUser(userId, sharedObjectDto.ObjectId);
            var entityToDelete = UnitOfWork.Shared.Entity.FirstOrDefault(e => e.UserId == shareUserId && e.NoteId == sharedObjectDto.ObjectId);
            if (entityToDelete is null)
            {
                throw new BadRequestException("That entity doesn't exist.");
            }
            DeleteEntityTransaction<Shared> shareFolderTransaction = new(UnitOfWork.Shared, entityToDelete.Id);
            shareFolderTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.UnshareNote, userId, DateTime.UtcNow, sharedObjectDto.ObjectId);
        }

        private Folder GetFolderIfBelongsToUser(int userId, int folderId)
        {
            var spec = new FolderByIdWithNotesAndSharedSpecification(folderId, userId);
            var folder = UnitOfWork.Folders.GetBySpecification(spec).FirstOrDefault();
            if (folder is null)
            {
                throw new NotFoundException("That entity doesn't exist.");
            }
            if (folder.UserId != userId)
            {
                throw new ForbidException("Cannot operate someone else's folder.");
            }
            return folder;
        }

        private Note GetNoteIfBelongsToUser(int userId, int noteId)
        {
            var spec = new NoteByIdWithFileAndCategoriesAndSharedSpecification(noteId, userId);
            var note = UnitOfWork.Notes.GetBySpecification(spec).FirstOrDefault();
            if (note is null)
            {
                throw new NotFoundException("That entity doesn't exist.");
            }
            if (note.UserId != userId)
            {
                throw new ForbidException("Cannot operate someone else's note.");
            }
            return note;
        }

        private void GetUserIdAndShareUserId(SharedObjectDto sharedObjectDto, out int userId, out int? shareUserId)
        {
            if (sharedObjectDto.ShareMode == EShareMode.None)
            {
                throw new BadRequestException("Wrong share mode.");
            }
            userId = userContextService.GetUserId;
            if (string.IsNullOrEmpty(sharedObjectDto.Email))
            {
                shareUserId = null;
                return;
            }
            shareUserId = UnitOfWork.Users.Entity.FirstOrDefault(u => u.Email == sharedObjectDto.Email)?.Id;
            if (shareUserId == null && !string.IsNullOrEmpty(sharedObjectDto.Email))
            {
                throw new BadRequestException("That user doesn't exist.");
            }
            if (shareUserId == userId)
            {
                throw new BadRequestException("Cannot share resource to yourself.");
            }
        }
    }
}