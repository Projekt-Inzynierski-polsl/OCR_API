using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;
using OCR_API.Enums;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.UploadedModelDtos;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.FolderTransactions;
using OCR_API.Transactions.SharedTransactions;

namespace OCR_API.Services
{
    public interface ISharedService
    {
        IUnitOfWork UnitOfWork { get; }
        IEnumerable<FolderDto> GetAllFoldersByUserId(string jwtToken);
        IEnumerable<NoteDto> GetAllNotesByUserId(string jwtToken);
        void ShareFolder(string jwtToken, SharedObjectDto sharedObjectDto);
        void ShareNote(string jwtToken, SharedObjectDto sharedObjectDto);
        void UnshareFolder(string jwtToken, SharedObjectDto sharedObjectDto);
        void UnshareNote(string jwtToken, SharedObjectDto sharedObjectDto);
    }
    public class SharedService : ISharedService
    {
        public IUnitOfWork UnitOfWork { get; }

        private readonly IPasswordHasher<Folder> passwordHasher;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly UserActionLogger logger;

        public SharedService(IUnitOfWork unitOfWork, IPasswordHasher<Folder> passwordHasher, IMapper mapper, JwtTokenHelper jwtTokenHelper, UserActionLogger logger)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
            this.logger = logger;
        }

        public IEnumerable<FolderDto> GetAllFoldersByUserId(string jwtToken)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);

            var spec = new SharedFoldersWithNotesSpecification(userId);
            var folders = UnitOfWork.Shared.GetBySpecification(spec);
            var foldersDto = folders.Select(f => mapper.Map<FolderDto>(f)).ToList();

            return foldersDto;
        }

        public IEnumerable<NoteDto> GetAllNotesByUserId(string jwtToken)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);

            var spec = new SharedNotesWithFileAndCategoriesSpecification(userId);
            var notes = UnitOfWork.Shared.GetBySpecification(spec);
            var notesDto = notes.Select(f => mapper.Map<NoteDto>(f)).ToList();

            return notesDto;
        }

        public void ShareFolder(string jwtToken, SharedObjectDto sharedObjectDto)
        {
            GetUserIdAndShareUserId(jwtToken, sharedObjectDto, out int userId, out int? shareUserId);
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
            ShareTransaction shareFolderTransaction = new ShareFolderTransaction(UnitOfWork.Shared, (int)shareUserId, sharedObjectDto.ObjectId, sharedObjectDto.ShareMode);
            shareFolderTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.ShareFolder, userId, DateTime.UtcNow, sharedObjectDto.ObjectId);
        }

        public void ShareNote(string jwtToken, SharedObjectDto sharedObjectDto)
        {
            GetUserIdAndShareUserId(jwtToken, sharedObjectDto, out int userId, out int? shareUserId);
            Note Note = GetNoteIfBelongsToUser(userId, sharedObjectDto.ObjectId);
            ShareTransaction shareNoteTransaction = new ShareNoteTransaction(UnitOfWork.Shared, (int)shareUserId, sharedObjectDto.ObjectId, sharedObjectDto.ShareMode);
            shareNoteTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.ShareNote, userId, DateTime.UtcNow, sharedObjectDto.ObjectId);
        }

        public void UnshareFolder(string jwtToken, SharedObjectDto sharedObjectDto)
        {
            GetUserIdAndShareUserId(jwtToken, sharedObjectDto, out int userId, out int? shareUserId);
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

        public void UnshareNote(string jwtToken, SharedObjectDto sharedObjectDto)
        {
            GetUserIdAndShareUserId(jwtToken, sharedObjectDto, out int userId, out int? shareUserId);
            Note folder = GetNoteIfBelongsToUser(userId, sharedObjectDto.ObjectId);
            var entityToDelete = UnitOfWork.Shared.Entity.FirstOrDefault(e => e.UserId == shareUserId && e.NoteId == sharedObjectDto.ObjectId);
            if(entityToDelete is null)
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
                throw new UnauthorizedAccessException("Cannot operate someone else's folder.");
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
                throw new UnauthorizedAccessException("Cannot operate someone else's note.");
            }
            return note;
        }

        private void GetUserIdAndShareUserId(string jwtToken, SharedObjectDto sharedObjectDto, out int userId, out int? shareUserId)
        {
            if (sharedObjectDto.ShareMode == EShareMode.None)
            {
                throw new BadRequestException("Wrong share mode.");
            }
            userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            shareUserId = UnitOfWork.Users.Entity.FirstOrDefault(u => u.Email == sharedObjectDto.Email)?.Id;
            if (shareUserId == null)
            {
                throw new BadRequestException("That user doesn't exist.");
            }
        }
    }
}
