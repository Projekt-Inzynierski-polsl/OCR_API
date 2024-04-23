using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using OCR_API;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.Validators;
using OCR_API.Services;

namespace UnitTests
{
    [TestClass]
    public class NoteServiceTests
    {
        private readonly IPasswordHasher<User> userPasswordHasher;
        private readonly INoteService service;
        private readonly IAccountService accountService;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly PasswordHasher<Folder> folderPasswordHasher;
        private readonly FolderService folderService;
        private readonly IValidator<AddNoteDto> addNoteValidator;
        private readonly IValidator<UpdateNoteDto> updateNoteValidator;
        private IUnitOfWork unitOfWork;
        private readonly UserActionLogger logger;
        public NoteServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            userPasswordHasher = new PasswordHasher<User>();
            folderPasswordHasher = new PasswordHasher<Folder>();
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            addNoteValidator = new AddNoteDtoValidator(unitOfWork);
            updateNoteValidator = new UpdateNoteDtoValidator(unitOfWork);
            logger = new UserActionLogger(unitOfWork);
            service = new NoteService(unitOfWork, mapper, jwtTokenHelper, logger);
            folderService = new FolderService(unitOfWork, folderPasswordHasher, mapper, jwtTokenHelper, logger);
            accountService = new AccountService(unitOfWork, userPasswordHasher, mapper, jwtTokenHelper, logger);
        }

        public string SetUpGetToken()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "TestPassword" };
            string token = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            return token;
        }
        [TestMethod]
        public void CreateNote_WithExistFolder_SuccessfullyCreated()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            folderService.CreateFolder(token, addFolderDto);
            string noteName = "TestNote";
            string content = "Test";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test"});
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            NoteDto noteDto = service.GetById(token, 1);
            Assert.IsNotNull(noteDto);
            Assert.AreEqual(noteName, noteDto.Name);
            Assert.IsFalse(noteDto.IsPrivate);
            Assert.AreEqual(content, noteDto.Content);
            Assert.AreEqual(1, noteDto.FolderId);
        }
        [TestMethod]
        public void CreateNote_WithoutFolder_SuccessfullyCreated()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            folderService.CreateFolder(token, addFolderDto);
            string noteName = "TestNote";
            string content = "Test";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = null };
            service.CreateNote(token, addNoteDto);
            NoteDto noteDto = service.GetById(token, 1);
            Assert.IsNotNull(noteDto);
            Assert.AreEqual(noteName, noteDto.Name);
            Assert.IsFalse(noteDto.IsPrivate);
            Assert.AreEqual(content, noteDto.Content);
            Assert.AreEqual(0, noteDto.FolderId);
        }
        [TestMethod]
        public void CreateNote_WithNonExistentFolder_ValidationFails()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            int nonExistentFolderId = 999;
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = nonExistentFolderId };

            var validationResult = addNoteValidator.Validate(addNoteDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void CreateNote_WithNonExistentNoteFileId_ValidationFails()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            int nonExistentNoteFileId = 999;
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = nonExistentNoteFileId, FolderId = null };

            var validationResult = addNoteValidator.Validate(addNoteDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void CreateNote_WithDuplicateName_ValidationFails()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            folderService.CreateFolder(token, addFolderDto);
            int existingFolderId = 1;
            AddNoteDto addNoteDto1 = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = existingFolderId };
            service.CreateNote(token, addNoteDto1);
            AddNoteDto addNoteDto2 = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = existingFolderId };

            var validationResult = addNoteValidator.Validate(addNoteDto2);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void CreateNote_WithNonUserOwnedFolder_ThrowsException()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            folderService.CreateFolder(token, addFolderDto);
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1};

            Assert.ThrowsException<BadRequestException>(() => service.CreateNote(token2, addNoteDto));
        }

        [TestMethod]
        public void GetAll_ReturnsNotesDto()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test2" });
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test3" });
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test4" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = true, NoteFileId = 1, FolderId = null };
            service.CreateNote(token, addNoteDto);
            string noteName2 = "TestNote2";
            string content2 = "my2";
            addNoteDto = new AddNoteDto() { Name = noteName2, Content = content2, IsPrivate = false, NoteFileId = 2, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            string noteName3 = "TestNote2";
            string content3 = "my3";
            addNoteDto = new AddNoteDto() { Name = noteName3, Content = content3, IsPrivate = false, NoteFileId = 3, FolderId = 1 };
            service.CreateNote(token, addNoteDto);


            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            folderService.CreateFolder(token2, addFolderDto);
            addNoteDto = new AddNoteDto() { Name = noteName2, Content = content2, IsPrivate = false, NoteFileId = 4, FolderId = 2 };
            service.CreateNote(token2, addNoteDto);

            var notes = service.GetAll(token);
            Assert.IsNotNull(notes);
            Assert.AreEqual(3, notes.Count());
            var enumeratedNotes = notes.ToList();
            Assert.AreEqual(noteName, enumeratedNotes[0].Name);
            Assert.AreEqual(content, enumeratedNotes[0].Content);
            Assert.AreEqual(noteName2, enumeratedNotes[1].Name);
            Assert.AreEqual(content2, enumeratedNotes[1].Content);
            Assert.AreEqual(noteName3, enumeratedNotes[2].Name);
            Assert.AreEqual(content3, enumeratedNotes[2].Content);

            var notes2 = service.GetAll(token2);
            Assert.IsNotNull(notes2);
            Assert.AreEqual(1, notes2.Count());
            var enumeratedNotes2 = notes2.ToList();
            Assert.AreEqual(noteName2, enumeratedNotes2[0].Name);
            Assert.AreEqual(content2, enumeratedNotes2[0].Content);
        }

        [TestMethod]
        public void DeleteNote_WithValidNoteId_SuccessfullyDeleted()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = true, NoteFileId = 1, FolderId = null };
            service.CreateNote(token, addNoteDto);

            service.DeleteNote(token, 1);

            Assert.ThrowsException<NotFoundException>(() => service.GetById(token, 1));
        }

        [TestMethod]
        public void DeleteNote_WithNonExistentNoteId_ThrowsException()
        {
            string token = SetUpGetToken();
            int nonExistentNoteId = 999;

            Assert.ThrowsException<NotFoundException>(() => service.DeleteNote(token, nonExistentNoteId));
        }

        [TestMethod]
        public void DeleteNote_WithNoteIdFromDifferentUser_ThrowsException()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            folderService.CreateFolder(token, addFolderDto);

            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = true, NoteFileId = 1, FolderId = null };
            service.CreateNote(token, addNoteDto);

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = true, NoteFileId = 1, FolderId = null };
            service.CreateNote(token2, addNoteDto);

            Assert.ThrowsException<UnauthorizedAccessException>(() => service.DeleteNote(token, 2)); 
        }
        [TestMethod]
        public void UpdateNote_WithValidData_SuccessfullyUpdated()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string updatedNoteName = "UpdatedNote";
            bool updatedIsPrivateValue = false;
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = true, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1;

            UpdateNoteDto updateNoteDto = new UpdateNoteDto() { Name = updatedNoteName, IsPrivate = updatedIsPrivateValue };
            service.UpdateNote(token, noteId, updateNoteDto);

            NoteDto updatedNote = service.GetById(token, noteId);
            Assert.IsNotNull(updatedNote);
            Assert.AreEqual(updatedNoteName, updatedNote.Name);
            Assert.AreEqual(updatedIsPrivateValue, updatedNote.IsPrivate);
        }

        [TestMethod]
        public void UpdateNote_WithTakenName_ValidationFails()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string existingNoteName = "ExistingNote";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test2" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = existingNoteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            AddNoteDto addNoteDto2 = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 2, FolderId = 1 };
            service.CreateNote(token, addNoteDto2);

            UpdateNoteDto updateNoteDto = new UpdateNoteDto() { Name = existingNoteName, IsPrivate = false, Content = content };
            var validationResult = updateNoteValidator.Validate(updateNoteDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void UpdateNote_WithNonExistentNoteId_ThrowsException()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string updatedNoteName = "UpdatedNote";
            bool updatedIsPrivateValue = false;
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = true, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int nonExistentNoteId = 999;

            UpdateNoteDto updateNoteDto = new UpdateNoteDto() { Name = updatedNoteName, IsPrivate = updatedIsPrivateValue };
            Assert.ThrowsException<NotFoundException>(() => service.UpdateNote(token, nonExistentNoteId, updateNoteDto));
        }

        [TestMethod]
        public void UpdateNote_WithNoteIdFromDifferentUser_ThrowsException()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            string updatedNoteName = "UpdatedNote";
            bool updatedIsPrivateValue = false;
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = true, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);

            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = "TestFolder2", IconPath = "icons/my2.png" };
            folderService.CreateFolder(token2, addFolderDto2);
            AddNoteDto addNoteDto2 = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = true, NoteFileId = 1, FolderId = 2 };
            service.CreateNote(token2, addNoteDto2);

            UpdateNoteDto updateNoteDto = new UpdateNoteDto() { Name = updatedNoteName, IsPrivate = updatedIsPrivateValue };
            Assert.ThrowsException<UnauthorizedAccessException>(() => service.UpdateNote(token, 2, updateNoteDto));
        }

        [TestMethod]
        public void ChangeNoteFolder_WithValidData_SuccessfullyChanged()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = "TestFolder2", IconPath = "icons/my2.png" };
            folderService.CreateFolder(token, addFolderDto2);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1;
            int targetFolderId = 2;

            ChangeNoteFolderDto changeNoteFolderDto = new ChangeNoteFolderDto() { FolderId = targetFolderId };
            service.ChangeNoteFolder(token, noteId, changeNoteFolderDto);

            NoteDto noteAfterChange = service.GetById(token, noteId);
            Assert.IsNotNull(noteAfterChange);
            Assert.AreEqual(targetFolderId, noteAfterChange.FolderId);
        }

        [TestMethod]
        public void ChangeNoteFolder_WithNullFolderId_AssertionFails()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1;

            ChangeNoteFolderDto changeNoteFolderDto = new ChangeNoteFolderDto() { FolderId = null };
            service.ChangeNoteFolder(token, noteId, changeNoteFolderDto);

            NoteDto noteAfterChange = service.GetById(token, noteId);
            Assert.IsNotNull(noteAfterChange);
            Assert.AreEqual(0, noteAfterChange.FolderId);
        }

        [TestMethod]
        public void ChangeNoteFolder_WithNonExistentFolderId_ThrowsException()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1;
            int nonExistentFolderId = 999;

            ChangeNoteFolderDto changeNoteFolderDto = new ChangeNoteFolderDto() { FolderId = nonExistentFolderId };
            Assert.ThrowsException<NotFoundException>(() => service.ChangeNoteFolder(token, noteId, changeNoteFolderDto));
        }

        [TestMethod]
        public void ChangeNoteFolder_WithFolderIdFromDifferentUser_ThrowsException()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = "TestFolder2", IconPath = "icons/my2.png" };
            folderService.CreateFolder(token2, addFolderDto2);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1; 

            ChangeNoteFolderDto changeNoteFolderDto = new ChangeNoteFolderDto() { FolderId = 2 };
            Assert.ThrowsException<BadRequestException>(() => service.ChangeNoteFolder(token, noteId, changeNoteFolderDto));
        }

        [TestMethod]
        public void UpdateNoteCategories_WithValidData_SuccessfullyUpdated()
        {
            string token = SetUpGetToken();
            string categoryName1 = "Category1";
            string categoryName2 = "Category2";
            NoteCategory noteCategory1 = new NoteCategory() { Name = categoryName1, UserId = 1 };
            NoteCategory noteCategory2 = new NoteCategory() { Name = categoryName2, UserId = 1 };
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            unitOfWork.NoteCategories.Add(noteCategory1);
            unitOfWork.NoteCategories.Add(noteCategory2);
            unitOfWork.Commit();
            string noteName = "TestNote";
            string content = "Test";
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1;
            int[] categoriesIds = new int[] { 1, 2 };

            UpdateNoteCategoriesDto updateNoteCategoriesDto = new UpdateNoteCategoriesDto() { CategoriesIds = categoriesIds };
            service.UpdateNoteCategories(token, noteId, updateNoteCategoriesDto);

            NoteDto noteAfterUpdate = service.GetById(token, noteId);
            Assert.IsNotNull(noteAfterUpdate);
            Assert.AreEqual(categoriesIds.Length, noteAfterUpdate.Categories.Count());
        }

        [TestMethod]
        public void UpdateNoteCategories_WithEmptyCategoriesArray_SuccessfullyRemovedAllCategories()
        {
            string token = SetUpGetToken();
            string categoryName = "Category1";
            NoteCategory noteCategory = new NoteCategory() { Name = categoryName, UserId = 1 };
            unitOfWork.NoteCategories.Add(noteCategory);
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            unitOfWork.Commit();
            string noteName = "TestNote";
            string content = "Test";
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1;

            UpdateNoteCategoriesDto updateNoteCategoriesDto = new UpdateNoteCategoriesDto() { CategoriesIds = Array.Empty<int>() };
            service.UpdateNoteCategories(token, noteId, updateNoteCategoriesDto);

            NoteDto noteAfterUpdate = service.GetById(token, noteId);
            Assert.IsNotNull(noteAfterUpdate);
            Assert.AreEqual(0, noteAfterUpdate.Categories.Count());
        }

        [TestMethod]
        public void UpdateNoteCategories_WithNonExistentCategoryId_ThrowsException()
        {
            string token = SetUpGetToken();
            string noteName = "TestNote";
            string content = "Test";
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1;
            int nonExistentCategoryId = 999;

            UpdateNoteCategoriesDto updateNoteCategoriesDto = new UpdateNoteCategoriesDto() { CategoriesIds = new int[] { nonExistentCategoryId } };
            Assert.ThrowsException<NotFoundException>(() => service.UpdateNoteCategories(token, noteId, updateNoteCategoriesDto));
        }

        [TestMethod]
        public void UpdateNoteCategories_WithMixedCategoryIds_SuccessfullyUpdatedAndNonExistentCategory_ThrowsException()
        {
            string token = SetUpGetToken();
            string categoryName = "Category1";
            NoteCategory noteCategory = new NoteCategory() { Name = categoryName, UserId = 1 };
            unitOfWork.NoteCategories.Add(noteCategory);
            unitOfWork.NoteFiles.Add(new NoteFile() { Path = "test" });
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            folderService.CreateFolder(token, addFolderDto);
            string noteName = "TestNote";
            string content = "Test";
            AddNoteDto addNoteDto = new AddNoteDto() { Name = noteName, Content = content, IsPrivate = false, NoteFileId = 1, FolderId = 1 };
            service.CreateNote(token, addNoteDto);
            int noteId = 1; 
            int nonExistentCategoryId = 999;

            UpdateNoteCategoriesDto updateNoteCategoriesDto = new UpdateNoteCategoriesDto() { CategoriesIds = new int[] { 1, nonExistentCategoryId } };
            Assert.ThrowsException<NotFoundException>(() => service.UpdateNoteCategories(token, noteId, updateNoteCategoriesDto));
        }
    }
}
