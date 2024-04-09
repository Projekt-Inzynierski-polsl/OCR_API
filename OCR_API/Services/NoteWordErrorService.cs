using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.UploadedModelDtos;
using OCR_API.Specifications;
using OCR_API.Transactions;
using System.IO.Compression;
using System.Text;

namespace OCR_API.Services
{

    public interface INoteWordErrorService
    {
        public IUnitOfWork UnitOfWork { get; }
        IEnumerable<NoteWordErrorDto> GetAll();
        IEnumerable<NoteWordErrorDto> GetAllForUser(int userId);
        NoteWordErrorDto GetById(int errorId);
        void DeleteById(int errorId);
        void DeleteAll();
        MemoryStream DownloadErrors();
    }

    public class NoteWordErrorService : INoteWordErrorService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly UserActionLogger logger;
        private string OCR_ERRORS_DIRECTORY_PATH = Path.Combine(Directory.GetCurrentDirectory(), "ocr_errors");

        public NoteWordErrorService(IUnitOfWork unitOfWork, IMapper mapper, JwtTokenHelper jwtTokenHelper, UserActionLogger logger)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
            this.logger = logger;
        }

        public IEnumerable<NoteWordErrorDto> GetAll()
        {
            var errors = UnitOfWork.NoteWordErrors.GetAll().Select(f => mapper.Map<NoteWordErrorDto>(f)).ToList();
            return errors;
        }
        public IEnumerable<NoteWordErrorDto> GetAllForUser(int userId)
        {
            var error = UnitOfWork.NoteWordErrors.GetAllByUser(userId).Select(f => mapper.Map<NoteWordErrorDto>(f)).ToList();
            return error;
        }
        public NoteWordErrorDto GetById(int errorId)
        {
            var error = UnitOfWork.NoteWordErrors.GetById(errorId);
            var errorDto = mapper.Map<NoteWordErrorDto>(error);

            return errorDto;

        }

        public void DeleteById(int errorId)
        {
            DeleteEntityTransaction<NoteWordError> deleteUserTransaction = new(UnitOfWork.NoteWordErrors, errorId);
            deleteUserTransaction.Execute();
            UnitOfWork.Commit();
        }

        public void DeleteAll()
        {
            TruncateTableTransaction<NoteWordError> truncateTableTransaction = new(UnitOfWork.NoteWordErrors);
            truncateTableTransaction.Execute();
            try
            {
                string[] files = Directory.GetFiles(OCR_ERRORS_DIRECTORY_PATH);

                foreach (string file in files)
                {
                    File.Delete(file);
                }

                string[] directories = Directory.GetDirectories(OCR_ERRORS_DIRECTORY_PATH);

                foreach (string directory in directories)
                {
                    Directory.Delete(directory, true);
                }

            }
            catch
            {
                throw new Exception("Napotkano błąd podczas usuwania plików.");
            }

            UnitOfWork.Commit();
    }

        public MemoryStream DownloadErrors()
        {
            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var ocrErrorsFolderEntry = archive.CreateEntry("OCR_Errors_Folder/");
                AddFolderContentsToZip(OCR_ERRORS_DIRECTORY_PATH, ocrErrorsFolderEntry);

                var noteWordErrors = UnitOfWork.NoteWordErrors.GetAll();

                var csvContent = new StringBuilder();
                csvContent.AppendLine("Id,FileId,UserId,CorrectContent");

                foreach (var error in noteWordErrors)
                {
                    csvContent.AppendLine($"{error.Id},{error.FileId},{error.UserId},{error.CorrectContent}");
                }

                var csvEntry = archive.CreateEntry("NoteWordErrors.csv");
                using (var writer = new StreamWriter(csvEntry.Open()))
                {
                    writer.Write(csvContent.ToString());
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        private void AddFolderContentsToZip(string folderPath, ZipArchiveEntry folderEntry)
        {
            foreach (var file in Directory.GetFiles(folderPath))
            {
                var fileName = Path.GetFileName(file);
                var fileEntry = folderEntry.Archive.CreateEntryFromFile(file, fileName);
            }

            foreach (var subDirectory in Directory.GetDirectories(folderPath))
            {
                var subDirectoryName = Path.GetFileName(subDirectory);
                var subDirectoryEntry = folderEntry.Archive.CreateEntry(subDirectoryName + "/");
                AddFolderContentsToZip(subDirectory, subDirectoryEntry);
            }
        }
    }
   
}
