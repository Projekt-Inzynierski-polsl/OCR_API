using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Services;
using OCR_API;
using OCR_API.ModelsDto.Validators;
using Newtonsoft.Json.Linq;
using OCR_API.Exceptions;
using OCR_API.Logger;

namespace UnitTests
{
    [TestClass]
    public class FolderServiceTests
    {
        private readonly IPasswordHasher<Folder> folderPasswordHasher;
        private readonly IPasswordHasher<User> userPasswordHasher;
        private readonly IFolderService service;
        private readonly IAccountService accountService;
        private readonly IValidator<AddFolderDto> addFolderValidator;
        private readonly IValidator<UpdateFolderDto> updateFolderValidator;
        private readonly IValidator<ConfirmedPasswordDto> confirmedPasswordValidator;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private IUnitOfWork unitOfWork;
        private UserActionLogger logger;
        private PaginationService paginationService;
        private IUserContextService userContextService;

        public FolderServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            folderPasswordHasher = new PasswordHasher<Folder>();
            userPasswordHasher = new PasswordHasher<User>();
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            addFolderValidator = new AddFolderDtoValidator(unitOfWork);
            updateFolderValidator = new UpdateFolderDtoValidator(unitOfWork);
            confirmedPasswordValidator = new ConfirmedPasswordDtoValidator();
            logger = new UserActionLogger(unitOfWork);
            paginationService = new();
            userContextService = Helper.CreateMockIUserContextService();
            service = new FolderService(unitOfWork, folderPasswordHasher, mapper, logger, paginationService, userContextService);
            accountService = new AccountService(unitOfWork, userPasswordHasher, mapper, jwtTokenHelper, logger, userContextService);
        }

        [TestMethod]
        public void AddFolder_WithoutPassword_SuccessfullyAdded()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            service.CreateFolder(addFolderDto);
            var folders = unitOfWork.Folders.GetAll();
            Assert.AreEqual(1, folders.Count);
            var folder = unitOfWork.Folders.GetById(1);
            Assert.IsNotNull(folder);
            Assert.AreEqual(name, folder.Name);
            Assert.AreEqual(iconPath, folder.IconPath);
            Assert.AreEqual(1, folder.UserId);
            Assert.AreEqual(null, folder.PasswordHash);
        }

        [TestMethod]
        public void AddFolder_WithCorrectPassword_SuccessfullyAdded()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            var folders = unitOfWork.Folders.GetAll();
            Assert.AreEqual(1, folders.Count);
            var folder = unitOfWork.Folders.GetById(1);
            Assert.IsNotNull(folder);
            Assert.AreEqual(name, folder.Name);
            Assert.AreEqual(iconPath, folder.IconPath);
            Assert.AreEqual(1, folder.UserId);
            var result = folderPasswordHasher.VerifyHashedPassword(folder, folder.PasswordHash, password);
            Assert.AreEqual(result, PasswordVerificationResult.Success);

        }
        [TestMethod]
        public void AddFolder_WithWrongConfirmedPassword_ValidationFails()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            string confirmedPassword = "123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = confirmedPassword };
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void AddFolder_WithWrongPassword_ValidationFails()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void AddFolder_WithoutName_ValidationFails()
        {
            Helper.RegisterAccount(accountService);
            string iconPath = "icons/my.png";
            string password = "123";
            AddFolderDto addFolderDto = new AddFolderDto() { IconPath = iconPath, Password = password, ConfirmedPassword = password };
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void AddFolder_WithTakenName_ValidationFails()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsTrue(validationResult.IsValid);
            service.CreateFolder(addFolderDto);
            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            var validationResult2 = addFolderValidator.Validate(addFolderDto2);
            Assert.IsFalse(validationResult2.IsValid);
        }
        [TestMethod]
        public void GetAll__ReturnsFoldersDto()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            string name2 = "TestFolder2";
            string iconPath2 = "icons/my2.png";
            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = name2, IconPath = iconPath2 };
            service.CreateFolder(addFolderDto2);
            string name3 = "TestFolder3";
            string iconPath3 = "icons/my.png";
            AddFolderDto addFolderDto3 = new AddFolderDto() { Name = name3, IconPath = iconPath3 };
            service.CreateFolder(addFolderDto3);

            Helper.RegisterAccount(accountService, "testUser2@dto.pl", "TestUser2", "TestPassword");

            AddFolderDto addFolderDto4 = new AddFolderDto() { Name = name, IconPath = iconPath };
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            service.CreateFolder(addFolderDto4);
            var parameters = new GetAllQuery() { PageNumber = 1, PageSize = 100 };
            Helper.ChangeIdInIUserContextService(userContextService, 1);
            var folders = service.GetAll(parameters);
            Assert.IsNotNull(folders);
            Assert.AreEqual(3, folders.Items.Count);
            var enumeratedFolders = folders.Items;
            Assert.AreEqual(name, enumeratedFolders[0].Name);
            Assert.AreEqual(iconPath, enumeratedFolders[0].IconPath);
            Assert.AreEqual(name2, enumeratedFolders[1].Name);
            Assert.AreEqual(iconPath2, enumeratedFolders[1].IconPath);
            Assert.AreEqual(name3, enumeratedFolders[2].Name);
            Assert.AreEqual(iconPath3, enumeratedFolders[2].IconPath);
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            var folders2 = service.GetAll(parameters);
            Assert.IsNotNull(folders2);
            Assert.AreEqual(1, folders2.Items.Count);
            var enumeratedFolders2 = folders2.Items;
            Assert.AreEqual(name, enumeratedFolders2[0].Name);
            Assert.AreEqual(iconPath, enumeratedFolders2[0].IconPath);
        }

        [TestMethod]
        public void GetById_WithoutPassword_ReturnsFolderDto()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            service.CreateFolder(addFolderDto);
            FolderDto folder = service.GetById(1);
            Assert.IsNotNull(folder);
            Assert.AreEqual(name, folder.Name);
            Assert.AreEqual(iconPath, folder.IconPath);
            Assert.IsFalse(folder.HasPassword);
        }

        [TestMethod]
        public void GetById_WithPassword_ReturnsFolderDto()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            PasswordDto passwordDto = new() { Password = password };
            FolderDto folder = service.GetById(1, passwordDto);
            Assert.IsNotNull(folder);
            Assert.AreEqual(name, folder.Name);
            Assert.AreEqual(iconPath, folder.IconPath);
            Assert.IsTrue(folder.HasPassword);
        }

        [TestMethod]
        public void GetById_WithWrongPassword_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            PasswordDto passwordDto = new() { Password = "123" };
            Assert.ThrowsException<BadRequestException>(() => service.GetById(1, passwordDto));
        }

        [TestMethod]
        public void GetById_WithWrongToken_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            PasswordDto passwordDto = new() { Password = "123" };
            Helper.RegisterAccount(accountService, "testUser2@dto.pl", "TestUser2", "TestPassword");
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            Assert.ThrowsException<ForbidException>(() => service.GetById(1, passwordDto));
        }
        [TestMethod]
        public void DeleteFolder_WithoutPassword_WithCorrectIdAndPassword_SuccessfullyDeleted()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath};
            service.CreateFolder(addFolderDto);
            FolderDto folder = service.GetById(1);
            Assert.IsNotNull(folder);
            service.DeleteFolder(1);
            var parameters = new GetAllQuery() { PageNumber = 1, PageSize = 100 };
            var folders = service.GetAll(parameters);
            Assert.IsNotNull(folders);
            Assert.AreEqual(0, folders.Items.Count());
        }
        [TestMethod]
        public void DeleteFolder_WithPassword_WithCorrectIdAndPassword_SuccessfullyDeleted()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            PasswordDto passwordDto = new() { Password = password };
            FolderDto folder = service.GetById(1, passwordDto);
            Assert.IsNotNull(folder);
            service.DeleteFolder(1, passwordDto);
            var parameters = new GetAllQuery() { PageNumber = 1, PageSize = 100 };
            var folders = service.GetAll(parameters);

            Assert.IsNotNull(folders);
            Assert.AreEqual(0, folders.Items.Count());
        }
        [TestMethod]
        public void DeleteFolder_WithPassword_WithCorrectIdAndWrongPassword_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            string wrongPassword = "123";
            PasswordDto passwordDto = new() { Password = wrongPassword };

            Assert.ThrowsException<BadRequestException>(() => service.DeleteFolder(1, passwordDto));
        }
        [TestMethod]
        public void DeleteFolder_WithPassword_WithCorrectIdAndWithoutPassword_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);

            Assert.ThrowsException<BadRequestException>(() => service.DeleteFolder(1));
        }
        [TestMethod]
        public void DeleteFolder_WithPassword_WithWrongId_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            Helper.RegisterAccount(accountService, "testUser2@dto.pl", "TestUser2", "TestPassword");
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            Assert.ThrowsException<ForbidException>(() => service.DeleteFolder(1));
        }
        [TestMethod]
        public void UpdateFolder_WithCorrectData_SuccessfullyUpdated()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath};
            service.CreateFolder(addFolderDto);
            string newName = "Update";
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = newName, IconPath = newIcon };
            service.UpdateFolder(1, updateFolderDto);
            var folder = service.GetById(1);
            Assert.IsNotNull(folder);
            Assert.AreEqual(newName, folder.Name);
            Assert.AreEqual(newIcon, folder.IconPath);
        }
        [TestMethod]
        public void UpdateFolder_WithPassword_WithCorrectData_SuccessfullyUpdated()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            string newName = "Update";
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = newName, IconPath = newIcon, PasswordToFolder = password };
            service.UpdateFolder(1, updateFolderDto);
            PasswordDto passwordDto = new() { Password = password };
            var folder = service.GetById(1, passwordDto);
            Assert.IsNotNull(folder);
            Assert.AreEqual(newName, folder.Name);
            Assert.AreEqual(newIcon, folder.IconPath);
        }
        [TestMethod]
        public void UpdateFolder_WithPassword_WithWrongPassword_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            string wrongPassword = "123";
            string newName = "Update";
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = newName, IconPath = newIcon, PasswordToFolder = wrongPassword};
            Assert.ThrowsException<BadRequestException>(() => service.UpdateFolder(1, updateFolderDto));
        }
        [TestMethod]
        public void UpdateFolder_WithWrongId_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath};
            service.CreateFolder(addFolderDto);
            Helper.RegisterAccount(accountService, "testUser2@dto.pl", "TestUser2", "TestPassword");
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            string newName = "Update";
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = newName, IconPath = newIcon };
            Assert.ThrowsException<ForbidException>(() => service.UpdateFolder(1, updateFolderDto));
        }
        [TestMethod]
        public void UpdateFolder_WithTakenName_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string name2 = "Folder2";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            service.CreateFolder(addFolderDto);
            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = name2, IconPath = iconPath };
            service.CreateFolder(addFolderDto2);
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = name, IconPath = newIcon };
            var validationResult = updateFolderValidator.Validate(updateFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void LockFolder_WithCorrectData_SuccessfullyLocked()
        {
            Helper.RegisterAccount(accountService);
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png"};
            service.CreateFolder(addFolderDto);

            ConfirmedPasswordDto confirmedPasswordDto = new ConfirmedPasswordDto() { Password = password, ConfirmedPassword = password };
            service.LockFolder(1, confirmedPasswordDto);

            var folder = unitOfWork.Folders.GetById(1);
            Assert.IsNotNull(folder);
            Assert.IsNotNull(folder.PasswordHash);
        }

        [TestMethod]
        public void LockFolder_WithIncorrectPassword_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png"};
            service.CreateFolder(addFolderDto);

            ConfirmedPasswordDto confirmedPasswordDto = new ConfirmedPasswordDto() { Password = "incorrectpassword", ConfirmedPassword = password };
            var validationResult = confirmedPasswordValidator.Validate(confirmedPasswordDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void LockFolder_OnAlreadyLockedFolder_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png", Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            ConfirmedPasswordDto confirmedPasswordDto = new ConfirmedPasswordDto() { Password = password, ConfirmedPassword = password };

            Assert.ThrowsException<BadRequestException>(() => service.LockFolder(1, confirmedPasswordDto));
        }

        [TestMethod]
        public void LockFolder_OnNotOwnedFolder_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png", Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            Helper.RegisterAccount(accountService, "testUser2@dto.pl", "TestUser2", "TestPassword");

            ConfirmedPasswordDto confirmedPasswordDto = new ConfirmedPasswordDto() { Password = password, ConfirmedPassword = password };
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            Assert.ThrowsException<ForbidException>(() => service.LockFolder(1, confirmedPasswordDto));
        }

        [TestMethod]
        public void UnlockFolder_WithCorrectData_SuccessfullyUnlocked()
        {
            Helper.RegisterAccount(accountService);
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png", Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            PasswordDto passwordDto = new PasswordDto() { Password = password };

            service.UnlockFolder(1, passwordDto);

            var folder = unitOfWork.Folders.GetById(1);
            Assert.IsNotNull(folder);
            Assert.IsNull(folder.PasswordHash);
        }

        [TestMethod]
        public void UnlockFolder_WithWrongPassword_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png", Password = password, ConfirmedPassword = password };
            service.CreateFolder(addFolderDto);
            PasswordDto wrongPasswordDto = new PasswordDto() { Password = "wrongpassword" };

            Assert.ThrowsException<BadRequestException>(() => service.UnlockFolder(1, wrongPasswordDto));
        }

        [TestMethod]
        public void UnlockFolder_OnAlreadyUnlockedFolder_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            service.CreateFolder(addFolderDto);
            PasswordDto passwordDto = new PasswordDto() { Password = "test123" };

            Assert.ThrowsException<BadRequestException>(() => service.UnlockFolder(1, passwordDto));
        }

        [TestMethod]
        public void UnlockFolder_WithWrongFolderId_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            service.CreateFolder(addFolderDto);
            int wrongFolderId = 2;
            PasswordDto passwordDto = new PasswordDto() { Password = "test123" };

            Assert.ThrowsException<NotFoundException>(() => service.UnlockFolder(wrongFolderId, passwordDto));
        }

        [TestMethod]
        public void UnlockFolder_WithWrongUserId_ThrowsException()
        {
            Helper.RegisterAccount(accountService);
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            service.CreateFolder(addFolderDto);
            PasswordDto passwordDto = new PasswordDto() { Password = "test123" };
            Helper.RegisterAccount(accountService, "testUser2@dto.pl", "TestUser2", "TestPassword");
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            Assert.ThrowsException<ForbidException>(() => service.UnlockFolder(1, passwordDto));
        }
    }
}
