﻿using AutoMapper;
using Newtonsoft.Json;
using OCR_API.Entities;
using OCR_API.Enums;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.BoundingBoxDtos;
using OCR_API.ModelsDto.NoteFileDtos;
using OCR_API.Specifications;
using OCR_API.Transactions.NoteFileTransactions;
using System.Text;

namespace OCR_API.Services
{
    public interface INoteFileService
    {
        public IUnitOfWork UnitOfWork { get; }

        PageResults<NoteFileDto> GetAllByUser(GetAllQuery queryParameters);

        NoteFileDto GetById(int categoryId);

        Task<NoteFileDto> UploadFileAsync(UploadFileDto uploadFileDto);
    }

    public class NoteFileService : INoteFileService
    {
        private const string UPLOADED_NOTE_FILE_DICTIONARY_PATH = "uploaded_files/notes";
        private string[] ALLOWED_FILE_EXTENSIONS = [".png", ".jpg"];

        private string OCR_MODEL_URL = EnvironmentSettings.ModelEnvironment switch
        {
            EEnvironment.Debug => "http://localhost:8053",
            EEnvironment.Production => "https://system-ocr-model-api.azurewebsites.net",
            _ => "http://model-ocr-api:5000"
        };


        private const string OCR_MODEL_UPLOAD_FILE_ENDPOINT = "/upload_image";
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly UserActionLogger logger;
        private readonly IPaginationService paginationService;
        private readonly IUserContextService userContextService;
        private readonly ImageCryptographer imageCryptographer;

        public NoteFileService(IUnitOfWork unitOfWork, IMapper mapper, UserActionLogger logger,
            IPaginationService paginationService, IUserContextService userContextService, ImageCryptographer imageCryptographer)
        {
            UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
            this.paginationService = paginationService;
            this.userContextService = userContextService;
            this.imageCryptographer = imageCryptographer;
        }

        public PageResults<NoteFileDto> GetAllByUser(GetAllQuery queryParameters)
        {
            var userId = userContextService.GetUserId;
            var spec = new NoteFilesByUserIdSpecification(userId, queryParameters.SearchPhrase);
            var noteFilesQuery = UnitOfWork.NoteFiles.GetBySpecification(spec);
            var result = paginationService.PreparePaginationResults<NoteFileDto, NoteFile>(queryParameters, noteFilesQuery, mapper);

            return result;
        }

        public NoteFileDto GetById(int noteFileId)
        {
            var userId = userContextService.GetUserId;
            NoteFile noteCategory = UnitOfWork.NoteFiles.GetByIdAndUserId(noteFileId, userId);
            var noteCategoryDto = mapper.Map<NoteFileDto>(noteCategory);

            return noteCategoryDto;
        }

        public async Task<NoteFileDto> UploadFileAsync(UploadFileDto uploadFileDto)
        {
            List<BoundingBoxDto> boundingBoxes = await TrySendImageToModelAsync(uploadFileDto);
            if (boundingBoxes == null)
            {
                throw new BadRequestException("Empty response from OCR model.");
            }

            NoteFile file = await TryUploadFile(uploadFileDto.Image, boundingBoxes);

            return mapper.Map<NoteFileDto>(file);
        }

        private async Task<List<BoundingBoxDto>> TrySendImageToModelAsync(UploadFileDto uploadFileDto)
        {
            using (var httpClient = new HttpClient())
            {
                var formData = new MultipartFormDataContent();

                var imageFile = uploadFileDto.Image;
                var imageContent = new StreamContent(imageFile.OpenReadStream());
                formData.Add(imageContent, "image", imageFile.FileName);

                formData.Add(new StringContent(uploadFileDto.BoundingBoxes, Encoding.UTF8, "application/json"), "json");
                formData.Add(new StringContent(await userContextService.GetJwtToken, Encoding.UTF8, "application/json"), "token");
                formData.Add(new StringContent(EnvironmentSettings.Environment.ToString(), Encoding.UTF8, "application/json"), "environment");

                var response = await httpClient.PostAsync(OCR_MODEL_URL + OCR_MODEL_UPLOAD_FILE_ENDPOINT, formData);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var boundingBoxesList = JsonConvert.DeserializeObject<List<BoundingBoxDto>>(responseContent);
                    return boundingBoxesList;
                }
                else
                {
                    throw new BadRequestException($"Model returned unsuccessful code. {EnvironmentSettings.Environment}");
                }
            }
        }

        private async Task<NoteFile> TryUploadFile(IFormFile image, List<BoundingBoxDto> boundingBoxes)
        {
            if (image.Length > 0)
            {
                var fileName = Path.GetFileName(image.FileName);

                var fileExtension = Path.GetExtension(fileName);
                if (!ALLOWED_FILE_EXTENSIONS.Contains(fileExtension))
                {
                    throw new BadRequestException("Wrong file extension");
                }
                var uploadedFile = SaveFileInDatabase(boundingBoxes, fileExtension);
                await SaveFileOnServer(image, uploadedFile.Id, fileExtension);
                return uploadedFile;
            }
            else
            {
                throw new BadRequestException("Empty file.");
            }
        }

        private NoteFile SaveFileInDatabase(List<BoundingBoxDto> boundingBoxesDto, string fileExtension)
        {
            var userId = userContextService.GetUserId;
            List<BoundingBox> boundingBoxes = boundingBoxesDto
                .Select(boundingBoxDto => mapper.Map<BoundingBox>(boundingBoxDto))
                .ToList();
            NoteFile fileToUpload = new() { BoundingBoxes = boundingBoxes, UserId = userId, Path = "" };
            UploadNoteFileTransaction uploadNoteFileTransaction = new UploadNoteFileTransaction(UnitOfWork.NoteFiles, fileToUpload);
            uploadNoteFileTransaction.Execute();
            UnitOfWork.Commit();
            var file = UnitOfWork.NoteFiles.GetById(uploadNoteFileTransaction.FileToUpload.Id);
            string filePath = Path.Combine(UPLOADED_NOTE_FILE_DICTIONARY_PATH, uploadNoteFileTransaction.FileToUpload.Id.ToString() + fileExtension);
            file.Path = filePath;
            UnitOfWork.Commit();
            logger.Log(EUserAction.UploadedFile, userId, DateTime.UtcNow, uploadNoteFileTransaction.FileToUpload.Id);
            return file;
        }

        private async Task SaveFileOnServer(IFormFile fileImage, int fileId, string fileExtension)
        {
            var image = imageCryptographer.ConvertIFormFileToImage(fileImage);
            var encryptedImageWitKey = await imageCryptographer.EncryptImageAsync(image);
            string filePath = Path.Combine(UPLOADED_NOTE_FILE_DICTIONARY_PATH, fileId.ToString() + fileExtension);
            if (!Directory.Exists(UPLOADED_NOTE_FILE_DICTIONARY_PATH))
            {
                Directory.CreateDirectory(UPLOADED_NOTE_FILE_DICTIONARY_PATH);
            }
            await File.WriteAllBytesAsync(filePath, encryptedImageWitKey.Item1);

            string hashedKeyString = BitConverter.ToString(encryptedImageWitKey.Item2).Replace("-", "");
            AddHashedKeyTransaction<NoteFile> addHashedKeyTransaction = new(UnitOfWork.NoteFiles, hashedKeyString, fileId);
            addHashedKeyTransaction.Execute();
            UnitOfWork.Commit();
            var userId = userContextService.GetUserId;
            logger.Log(EUserAction.CryptographyFile, userId, DateTime.UtcNow, fileId);
        }
    }
}