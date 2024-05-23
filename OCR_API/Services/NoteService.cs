using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
using iTextSharp.text;
using iTextSharp.text.pdf;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using OCR_API.Authorization;

namespace OCR_API.Services
{
    public interface INoteService
    {
        IUnitOfWork UnitOfWork { get; }

        PageResults<NoteDto> GetAllByUser(GetAllQuery queryParameters);
        NoteDto GetById(int noteId);
        IEnumerable<NoteDto> GetLastEdited(int amount);
        int CreateNote(AddNoteDto addNoteDto);
        void DeleteNote(int noteId);
        void UpdateNote(int noteId, UpdateNoteDto updateNoteDto);
        void ChangeNoteFolder(int noteId, ChangeNoteFolderDto changeNoteFolderDto);
        void UpdateNoteCategories(int noteId, UpdateNoteCategoriesDto updateNoteCategoriesFolderDto);
        string ExportPdfById(int noteId);
        string ExportDocxById(int noteId);
    }
    public class NoteService : INoteService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly UserActionLogger logger;
        private readonly IPaginationService queryParametersService;
        private readonly IUserContextService userContextService;

        public NoteService(IUnitOfWork unitOfWork, IMapper mapper, UserActionLogger logger, IPaginationService queryParametersService, 
            IUserContextService userContextService)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
            this.queryParametersService = queryParametersService;
            this.userContextService = userContextService;
        }

        public PageResults<NoteDto> GetAllByUser(GetAllQuery queryParameters)
        {
            var userId = userContextService.GetUserId;
            var spec = new NotesWithFileAndCategoriesSpecification(userId, queryParameters.SearchPhrase);
            var notesQuery = UnitOfWork.Notes.GetBySpecification(spec);
            var result = queryParametersService.PreparePaginationResults<NoteDto, Note>(queryParameters, notesQuery, mapper);

            return result;
        }

        public NoteDto GetById(int noteId)
        {
            var userId = userContextService.GetUserId;
            Note note = GetNoteIfBelongsToUser(userId, noteId);
            var noteDto = mapper.Map<NoteDto>(note);

            return noteDto;
        }

        public IEnumerable<NoteDto> GetLastEdited(int amount)
        {
            var userId = userContextService.GetUserId;
            EUserAction[] lastEditedActions = new[] { EUserAction.CreateNote, EUserAction.UpdateNote, EUserAction.ChangeNoteFolder, EUserAction.UpdateNoteCategories };
            var actionIds = lastEditedActions.Select(a => (int)a);
            var noteIds = UnitOfWork.UserLogs.Entity.ToList()
                .Where(f => f.UserId == userId && actionIds.Contains(f.Id))
                .TakeLast(amount)
                .Select(f => f.ObjectId).ToArray();

            var notes = UnitOfWork.Notes.Entity.ToList().Where(f => noteIds.Contains(f.Id)).ToList();
            var notesDto = notes.Select(f => mapper.Map<NoteDto>(f)).ToList();

            return notesDto;
        }

        public int CreateNote(AddNoteDto addNoteDto)
        {
            var userId = userContextService.GetUserId;
            NoteFile noteFile = UnitOfWork.NoteFiles.GetById(addNoteDto.NoteFileId);
            if(noteFile.UserId != userId) 
            {
                throw new BadRequestException("Cannot access to this file.");
            }

            Note noteToAdd = mapper.Map<Note>(addNoteDto); 
            AddNoteTransaction addNoteTransaction = new(UnitOfWork, userId, noteToAdd);
            addNoteTransaction.Execute();
            UnitOfWork.Commit();
            var newNoteId = addNoteTransaction.NoteToAdd.Id;
            logger.Log(EUserAction.CreateNote, userId, DateTime.UtcNow, newNoteId);
            return newNoteId;
        }

        public void DeleteNote(int noteId)
        {
            var userId = userContextService.GetUserId;
            Note noteToRemove = GetNoteIfBelongsToUser(userId, noteId);

            if (ResourceOperationAccess.CanEdit(noteToRemove, userId))
            {
                DeleteEntityTransaction<Note> deleteNoteTransaction = new(UnitOfWork.Notes, noteToRemove.Id);
                deleteNoteTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.DeleteNote, userId, DateTime.UtcNow, noteId);
            }
            else
            {
                throw new ForbidException("Cannot operate someone else's note.");
            }
        }

        public void UpdateNote(int noteId, UpdateNoteDto updateNoteDto)
        {
            var userId = userContextService.GetUserId;
            Note noteToUpdate = GetNoteIfBelongsToUser(userId, noteId);

            if (ResourceOperationAccess.CanEdit(noteToUpdate, userId))
            {
                UpdateNoteTransaction updateNoteTransaction = new(noteToUpdate, updateNoteDto.Name, updateNoteDto.Content, updateNoteDto.IsPrivate);
                updateNoteTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.UpdateNote, userId, DateTime.UtcNow, noteId);
            }
            else
            {
                throw new ForbidException("Cannot operate someone else's note.");
            }
        }

        public void ChangeNoteFolder(int noteId, ChangeNoteFolderDto changeNoteFolderDto)
        {
            var userId = userContextService.GetUserId;
            Note noteToMove = GetNoteIfBelongsToUser(userId, noteId);

            if (ResourceOperationAccess.CanEdit(noteToMove, userId))
            {
                ChangeNoteFolderTransaction changeNoteFolderTransaction = new(UnitOfWork, userId, noteToMove, changeNoteFolderDto.FolderId);
                changeNoteFolderTransaction.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.ChangeNoteFolder, userId, DateTime.UtcNow, noteId);
            }
            else
            {
                throw new ForbidException("Cannot operate someone else's note.");
            }
        }

        public void UpdateNoteCategories(int noteId, UpdateNoteCategoriesDto updateNoteCategoriesFolderDto)
        {
            var userId = userContextService.GetUserId;
            Note noteToUpdate = GetNoteIfBelongsToUser(userId, noteId);

            if (ResourceOperationAccess.CanEdit(noteToUpdate, userId))
            {
                UpdateNoteCategoriesTransaction updateNoteCategories = new(UnitOfWork, noteToUpdate, userId, updateNoteCategoriesFolderDto.CategoriesIds);
                updateNoteCategories.Execute();
                UnitOfWork.Commit();
                logger.Log(EUserAction.UpdateNoteCategories, userId, DateTime.UtcNow, noteId);
            }
            else
            {
                throw new ForbidException("Cannot operate someone else's note.");
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
            if (note.UserId != userId && !ResourceOperationAccess.IsShared(note, userId))
            {
                throw new ForbidException("Cannot operate someone else's note.");
            }
            return note;
        }

        public string ExportDocxById(int noteId)
        {
            var userId = userContextService.GetUserId;
            Note note = GetNoteIfBelongsToUser(userId, noteId);
            var noteContent = note.Content;
            var directoryPath = "./wwwroot";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var docxFilePath = $"{directoryPath}/{noteId}.docx";
            var fullFilePath = Path.GetFullPath(docxFilePath);
            if (File.Exists(fullFilePath))
            {
                return $"/{noteId}.docx";
            }
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(fullFilePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                Body body = mainPart.Document.AppendChild(new Body());
                var para = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                Run run = para.AppendChild(new Run());
                run.AppendChild(new Text(note.Name));
                string[] paragraphs = noteContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (string paragraphText in paragraphs)
                {
                    para = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                    run = para.AppendChild(new Run());
                    run.AppendChild(new Text(paragraphText));
                }

                mainPart.Document.Save();
            }
            return $"/{noteId}.docx";
        }

        public string ExportPdfById(int noteId)
        {
            var userId = userContextService.GetUserId;
            Note note = GetNoteIfBelongsToUser(userId, noteId);
            var noteContent = note.Content;
            var directoryPath = "./wwwroot";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var pdfFilePath = $"{directoryPath}/{noteId}.pdf";
            var fullFilePath = Path.GetFullPath(pdfFilePath);
            if (File.Exists(fullFilePath))
            {
                return $"/{noteId}.pdf";
            }
            using (var stream = new MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                PdfWriter.GetInstance(document, new FileStream(fullFilePath, FileMode.Create));

                document.Open();
                document.Add(new iTextSharp.text.Paragraph(note.Name));
                document.Add(new iTextSharp.text.Paragraph(noteContent));
                document.Close();
            }
            return $"/{noteId}.pdf";
        }
    }
}
