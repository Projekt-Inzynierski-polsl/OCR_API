using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using OCR_API.Entities;
using OCR_API.Enums;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.NoteTransactions;
using System.Linq;

namespace OCR_API.Services
{
    public interface INoteService
    {
        IUnitOfWork UnitOfWork { get; }

        IEnumerable<NoteDto> GetAll(string accessToken);
        NoteDto GetById(string accessToken, int noteId);
        IEnumerable<NoteDto> GetLastEdited(string accessToken, int amount);
        int CreateNote(string accessToken, AddNoteDto addNoteDto);
        void DeleteNote(string accessToken, int noteId);
        void UpdateNote(string accessToken, int noteId, UpdateNoteDto updateNoteDto);
        void ChangeNoteFolder(string accessToken, int noteId, ChangeNoteFolderDto changeNoteFolderDto);
        void UpdateNoteCategories(string accessToken, int noteId, UpdateNoteCategoriesDto updateNoteCategoriesFolderDto);
    }
    public class NoteService : INoteService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly UserActionLogger logger;

        public NoteService(IUnitOfWork unitOfWork, IMapper mapper, JwtTokenHelper jwtTokenHelper, UserActionLogger logger)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
            this.logger = logger;
        }

        public IEnumerable<NoteDto> GetAll(string jwtToken)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var spec = new NotesWithFileAndCategoriesSpecification(userId);
            var notes = UnitOfWork.Notes.GetBySpecification(spec);
            var notesDto = notes.Select(f => mapper.Map<NoteDto>(f)).ToList();

            return notesDto;
        }

        public NoteDto GetById(string jwtToken, int noteId)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Note note = GetNoteIfBelongsToUser(userId, noteId);
            var noteDto = mapper.Map<NoteDto>(note);

            return noteDto;
        }

        public IEnumerable<NoteDto> GetLastEdited(string jwtToken, int amount)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            EUserAction[] lastEditedActions = new[] { EUserAction.CreateNote, EUserAction.UpdateNote, EUserAction.ChangeNoteFolder, EUserAction.UpdateNoteCategories };
            var noteIds = UnitOfWork.UserLogs.Entity.Where(f => f.UserId == userId && lastEditedActions.Contains((EUserAction)f.ActionId)).TakeLast(amount).Select(f => f.ObjectId);
            var notes = UnitOfWork.Notes.Entity.Where(f => noteIds.Contains(f.Id));
            var notesDto = notes.Select(f => mapper.Map<NoteDto>(f)).ToList();

            return notesDto;
        }

        public int CreateNote(string jwtToken, AddNoteDto addNoteDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Note noteToAdd = mapper.Map<Note>(addNoteDto); 
            AddNoteTransaction addNoteTransaction = new(UnitOfWork, userId, noteToAdd);
            addNoteTransaction.Execute();
            UnitOfWork.Commit();
            var newNoteId = addNoteTransaction.NoteToAdd.Id;
            logger.Log(EUserAction.CreateNote, userId, DateTime.UtcNow, newNoteId);
            return newNoteId;
        }

        public void DeleteNote(string jwtToken, int noteId)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Note noteToRemove = GetNoteIfBelongsToUser(userId, noteId);

            if (CanEdit(noteToRemove, userId))
            {
                DeleteEntityTransaction<Note> deleteNoteTransaction = new(UnitOfWork.Notes, noteToRemove.Id);
                deleteNoteTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.DeleteNote, userId, DateTime.UtcNow, noteId);
            }
            else
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's note.");
            }
        }

        public void UpdateNote(string jwtToken, int noteId, UpdateNoteDto updateNoteDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Note noteToUpdate = GetNoteIfBelongsToUser(userId, noteId);

            if (CanEdit(noteToUpdate, userId))
            {
                UpdateNoteTransaction updateNoteTransaction = new(noteToUpdate, updateNoteDto.Name, updateNoteDto.Content, updateNoteDto.IsPrivate);
                updateNoteTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.UpdateNote, userId, DateTime.UtcNow, noteId);
            }
            else
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's note.");
            }
        }

        public void ChangeNoteFolder(string jwtToken, int noteId, ChangeNoteFolderDto changeNoteFolderDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Note noteToMove = GetNoteIfBelongsToUser(userId, noteId);

            if (CanEdit(noteToMove, userId))
            {
                ChangeNoteFolderTransaction changeNoteFolderTransaction = new(UnitOfWork, userId, noteToMove, changeNoteFolderDto.FolderId);
                changeNoteFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.ChangeNoteFolder, userId, DateTime.UtcNow, noteId);
            }
            else
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's note.");
            }
        }

        public void UpdateNoteCategories(string jwtToken, int noteId, UpdateNoteCategoriesDto updateNoteCategoriesFolderDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            Note noteToUpdate = GetNoteIfBelongsToUser(userId, noteId);

            if (CanEdit(noteToUpdate, userId))
            {
                UpdateNoteCategoriesTransaction updateNoteCategories = new(UnitOfWork, noteToUpdate, userId, updateNoteCategoriesFolderDto.CategoriesIds);
                updateNoteCategories.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.UpdateNoteCategories, userId, DateTime.UtcNow, noteId);
            }
            else
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's note.");
            }
        }

        private Note GetNoteIfBelongsToUser(int userId, int noteId)
        {
            var spec = new NoteByIdWithFileAndCategoriesAndSharedSpecification(noteId, userId);
            var note = UnitOfWork.Notes.GetBySpecification(spec).FirstOrDefault();
            if (note is null)
            {
                throw new NotFoundException("That entity doesn't exist.");
            }
            if (note.UserId != userId && !IsShared(note, userId))
            {
                throw new UnauthorizedAccessException("Cannot operate someone else's note.");
            }
            return note;
        }

        private bool IsShared(Note note, int userId)
        {
            return note.SharedObjects.Where(f => f.UserId == userId).Count() > 0;
        }

        private bool CanEdit(Note note, int userId)
        {
            return note.UserId == userId || note.SharedObjects.FirstOrDefault(f => f.UserId == userId).ModeId == (int)EShareMode.Edit;
        }
    }
}
