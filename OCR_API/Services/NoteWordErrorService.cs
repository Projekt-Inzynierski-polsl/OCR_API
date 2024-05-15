using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.BoundingBoxDtos;
using OCR_API.ModelsDto.NoteFileDtos;
using OCR_API.ModelsDto.UploadedModelDtos;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.NoteFileTransactions;
using OCR_API.Transactions.NoteWordErrorTransactions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using static iTextSharp.text.pdf.codec.TiffWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OCR_API.Services
{

    public interface INoteWordErrorService
    {
        public IUnitOfWork UnitOfWork { get; }
        PageResults<NoteWordErrorDto> GetAll(GetAllQuery queryParameters);
        PageResults<NoteWordErrorDto> GetAllForUser(int userId, GetAllQuery queryParameters);
        NoteWordErrorDto GetById(int errorId);
        Task<NoteWordErrorDto> AddErrorAsync(AddErrorDto addErrorDto);
        void DeleteById(int errorId);
        void DeleteAll();
        MemoryStream DownloadErrors();
        void AcceptError(int errorId);
    }

    public class NoteWordErrorService : INoteWordErrorService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly UserActionLogger logger;
        private readonly IUserContextService userContextService;
        private readonly ImageCryptographer imageCryptographer;
        private readonly IPaginationService queryParametersService;
        private const string OCR_ERRORS_DIRECTORY_PATH = "uploaded_files/errors";
        private const string FILE_EXTENSION = ".png";

        public NoteWordErrorService(IUnitOfWork unitOfWork, IMapper mapper, UserActionLogger logger,
            IUserContextService userContextService, ImageCryptographer imageCryptographer, IPaginationService queryParametersService)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
            this.userContextService = userContextService;
            this.imageCryptographer = imageCryptographer;
            this.queryParametersService = queryParametersService;
        }

        public PageResults<NoteWordErrorDto> GetAll(GetAllQuery queryParameters)
        {
            var errors = UnitOfWork.NoteWordErrors.GetAll().AsQueryable();
            var result = queryParametersService.PreparePaginationResults<NoteWordErrorDto, NoteWordError>(queryParameters, errors, mapper);
            return result;
        }
        public PageResults<NoteWordErrorDto> GetAllForUser(int userId, GetAllQuery queryParameters)
        {
            var errors = UnitOfWork.NoteWordErrors.GetAllByUser(userId).AsQueryable();
            var result = queryParametersService.PreparePaginationResults<NoteWordErrorDto, NoteWordError>(queryParameters, errors, mapper);
            return result;
        }
        public NoteWordErrorDto GetById(int errorId)
        {
            var error = UnitOfWork.NoteWordErrors.GetById(errorId);
            var errorDto = mapper.Map<NoteWordErrorDto>(error);

            return errorDto;
        }

        public async Task<NoteWordErrorDto> AddErrorAsync(AddErrorDto addErrorDto)
        {
            var userId = userContextService.GetUserId;
            NoteFile dbFile = UnitOfWork.NoteFiles.GetById(addErrorDto.FileId);
            if (dbFile.UserId != userId)
            {
                throw new BadRequestException("Cannot access to this file.");
            }

            var hashedKeyString = dbFile.HashedKey;
            hashedKeyString = hashedKeyString.Replace("-", "");
            byte[] hashedKeyBytes = Enumerable.Range(0, hashedKeyString.Length)
                                   .Where(x => x % 2 == 0)
                                   .Select(x => Convert.ToByte(hashedKeyString.Substring(x, 2), 16))
                                   .ToArray();


            var encryptedImage = File.ReadAllBytes(dbFile.Path);
            var decryptedImage = await imageCryptographer.DecryptImageAsync(encryptedImage, hashedKeyBytes);

            using (var imageStream = new MemoryStream(decryptedImage))
            {
                var image = Image.Load<Rgba32>(imageStream);

                int width = addErrorDto.RightX - addErrorDto.LeftX;
                int height = addErrorDto.RightY - addErrorDto.LeftY;
                var cutArea = new Rectangle(addErrorDto.LeftX, addErrorDto.LeftY, width, height);

                image.Mutate(x => x.Crop(cutArea));
                var uploadedFile = SaveFileInDatabase();
                var uplaodedError = SaveErrorInDatabase(addErrorDto, uploadedFile.Id, userId);
                await SaveFileOnServer(image, uploadedFile.Id);

                return mapper.Map<NoteWordErrorDto>(uplaodedError);
            }
        }

        private NoteWordError SaveErrorInDatabase(AddErrorDto addErrorDto, int fileId, int userId)
        {
            NoteWordError errorToAdd = mapper.Map<NoteWordError>(addErrorDto);
            errorToAdd.FileId = fileId;
            errorToAdd.UserId = userId;
            AddErrorTransaction addErrorTransaction = new AddErrorTransaction(UnitOfWork.NoteWordErrors, errorToAdd);
            addErrorTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.ReportError, userId, DateTime.UtcNow, addErrorTransaction.ErrorToAdd.Id);
            return addErrorTransaction.ErrorToAdd;
        }

        private ErrorCutFile SaveFileInDatabase()
        {
            UploadErrorFileTransaction uploadErrorFileTransaction = new UploadErrorFileTransaction(UnitOfWork.ErrorCutFiles, OCR_ERRORS_DIRECTORY_PATH, FILE_EXTENSION);
            uploadErrorFileTransaction.Execute();
            UnitOfWork.Commit();
            return uploadErrorFileTransaction.FileToUpload;
        }

        private async Task SaveFileOnServer(Image image, int fileId)
        {
            var encryptedImageWitKey = await imageCryptographer.EncryptImageAsync(image);
            string filePath = Path.Combine(OCR_ERRORS_DIRECTORY_PATH, fileId.ToString() + FILE_EXTENSION);
            await File.WriteAllBytesAsync(filePath, encryptedImageWitKey.Item1);

            string hashedKeyString = BitConverter.ToString(encryptedImageWitKey.Item2).Replace("-", "");
            AddHashedKeyTransaction<ErrorCutFile> addHashedKeyTransaction = new(UnitOfWork.ErrorCutFiles, hashedKeyString, fileId);
            addHashedKeyTransaction.Execute();
            UnitOfWork.Commit();
            var userId = userContextService.GetUserId;
            logger.Log(EUserAction.CryptographyFile, userId, DateTime.UtcNow, fileId);
        }

        public void DeleteById(int errorId)
        {
            var userId = userContextService.GetUserId;
            DeleteEntityTransaction<NoteWordError> deleteUserTransaction = new(UnitOfWork.NoteWordErrors, errorId);
            deleteUserTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.DeleteError, userId, DateTime.Now, errorId);
        }

        public void DeleteAll()
        {
            TruncateTableTransaction<NoteWordError> truncateTableTransaction = new(UnitOfWork.NoteWordErrors);
            truncateTableTransaction.Execute();
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

            UnitOfWork.Commit();
            var userId = userContextService.GetUserId;
            logger.Log(EUserAction.ClearErrorTable, userId, DateTime.Now);
        }

        public MemoryStream DownloadErrors()
        {
            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var ocrErrorsFolderEntry = archive.CreateEntry("OCR_Errors_Folder/");
                AddDecryptedImagesToZip(OCR_ERRORS_DIRECTORY_PATH, ocrErrorsFolderEntry);

                var noteWordErrors = UnitOfWork.NoteWordErrors.GetAll();

                var csvContent = new StringBuilder();
                csvContent.AppendLine("Id,FileId,UserId,CorrectContent");

                foreach (var error in noteWordErrors)
                {
                    csvContent.AppendLine($"{error.Id},{error.FileId},{error.UserId},{error.CorrectContent}");
                }

                var csvEntry = archive.CreateEntry("NoteWordErrors.csv");
                using var writer = new StreamWriter(csvEntry.Open());
                writer.Write(csvContent.ToString());
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var userId = userContextService.GetUserId;
            logger.Log(EUserAction.DownloadErrors, userId, DateTime.Now);
            return memoryStream;
        }

        private async Task AddDecryptedImagesToZip(string folderPath, ZipArchiveEntry folderEntry)
        {
            foreach (var file in Directory.GetFiles(folderPath))
            {
                var fileName = Path.GetFileName(file);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var dbFile = UnitOfWork.ErrorCutFiles.GetById(int.Parse(fileNameWithoutExtension));
                var hashedKeyString = dbFile.HashedKey;
                hashedKeyString = hashedKeyString.Replace("-", "");
                byte[] hashedKeyBytes = Enumerable.Range(0, hashedKeyString.Length)
                                     .Where(x => x % 2 == 0)
                                     .Select(x => Convert.ToByte(hashedKeyString.Substring(x, 2), 16))
                                     .ToArray();
                var encryptedImage = File.ReadAllBytes(file);
                var decryptedImage = await imageCryptographer.DecryptImageAsync(encryptedImage, hashedKeyBytes);
                using (MemoryStream ms = new MemoryStream(decryptedImage))
                {
                    var entry = folderEntry.Archive.CreateEntry(fileName);
                    using (Stream entryStream = entry.Open())
                    {
                        ms.CopyTo(entryStream);
                    }
                }
            }

            foreach (var subDirectory in Directory.GetDirectories(folderPath))
            {
                var subDirectoryName = Path.GetFileName(subDirectory);
                var subDirectoryEntry = folderEntry.Archive.CreateEntry(subDirectoryName + "/");
                await AddDecryptedImagesToZip(subDirectory, subDirectoryEntry);
            }
        }

        public void AcceptError(int errorId)
        {
            var userId = userContextService.GetUserId;
            var error = UnitOfWork.NoteWordErrors.GetById(errorId);
            AcceptErrorTransaction acceptErrorTransaction = new(error);
            UnitOfWork.Commit();
            logger.Log(EUserAction.AcceptError, userId, DateTime.UtcNow, errorId);
        }
    }
   
}
