using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Services;
using OCR_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCR_API.ModelsDto.Validators;
using Newtonsoft.Json.Linq;
using OCR_API.Exceptions;

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
            service = new FolderService(unitOfWork, folderPasswordHasher, mapper, jwtTokenHelper);
            accountService = new AccountService(unitOfWork, userPasswordHasher, mapper, jwtTokenHelper);
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
        public void AddFolder_WithoutPassword_SuccessfullyAdded()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            service.CreateFolder(token, addFolderDto);
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
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
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
            string iconPath = "icons/my.png";
            string password = "123";
            AddFolderDto addFolderDto = new AddFolderDto() { IconPath = iconPath, Password = password, ConfirmedPassword = password };
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void AddFolder_WithTakenName_ValidationFails()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsTrue(validationResult.IsValid);
            service.CreateFolder(token, addFolderDto);
            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            var validationResult2 = addFolderValidator.Validate(addFolderDto2);
            Assert.IsFalse(validationResult2.IsValid);
        }
        [TestMethod]
        public void GetAll__ReturnsFoldersDto()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            string name2 = "TestFolder2";
            string iconPath2 = "icons/my2.png";
            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = name2, IconPath = iconPath2 };
            service.CreateFolder(token, addFolderDto2);
            string name3 = "TestFolder3";
            string iconPath3 = "icons/my.png";
            AddFolderDto addFolderDto3 = new AddFolderDto() { Name = name3, IconPath = iconPath3 };
            service.CreateFolder(token, addFolderDto3);


            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            AddFolderDto addFolderDto4 = new AddFolderDto() { Name = name, IconPath = iconPath };
            service.CreateFolder(token2, addFolderDto4);

            var folders = service.GetAll(token);
            Assert.IsNotNull(folders);
            Assert.AreEqual(3, folders.Count());
            var enumeratedFolders = folders.ToList();
            Assert.AreEqual(name, enumeratedFolders[0].Name);
            Assert.AreEqual(iconPath, enumeratedFolders[0].IconPath);
            Assert.AreEqual(name2, enumeratedFolders[1].Name);
            Assert.AreEqual(iconPath2, enumeratedFolders[1].IconPath);
            Assert.AreEqual(name3, enumeratedFolders[2].Name);
            Assert.AreEqual(iconPath3, enumeratedFolders[2].IconPath);

            var folders2 = service.GetAll(token2);
            Assert.IsNotNull(folders2);
            Assert.AreEqual(1, folders2.Count());
            var enumeratedFolders2 = folders2.ToList();
            Assert.AreEqual(name, enumeratedFolders2[0].Name);
            Assert.AreEqual(iconPath, enumeratedFolders2[0].IconPath);
        }

        [TestMethod]
        public void GetById_WithoutPassword_ReturnsFolderDto()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            service.CreateFolder(token, addFolderDto);
            FolderDto folder = service.GetById(token, 1);
            Assert.IsNotNull(folder);
            Assert.AreEqual(name, folder.Name);
            Assert.AreEqual(iconPath, folder.IconPath);
            Assert.IsFalse(folder.HasPassword);
        }

        [TestMethod]
        public void GetById_WithPassword_ReturnsFolderDto()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            PasswordDto passwordDto = new() { Password = password };
            FolderDto folder = service.GetById(token, 1, passwordDto);
            Assert.IsNotNull(folder);
            Assert.AreEqual(name, folder.Name);
            Assert.AreEqual(iconPath, folder.IconPath);
            Assert.IsTrue(folder.HasPassword);
        }

        [TestMethod]
        public void GetById_WithWrongPassword_ThrowsException()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            PasswordDto passwordDto = new() { Password = "123" };
            Assert.ThrowsException<BadRequestException>(() => service.GetById(token, 1, passwordDto));
        }

        [TestMethod]
        public void GetById_WithWrongToken_ThrowsException()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            PasswordDto passwordDto = new() { Password = "123" };
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            Assert.ThrowsException<UnauthorizedAccessException>(() => service.GetById(token2, 1, passwordDto));
        }
        [TestMethod]
        public void DeleteFolder_WithoutPassword_WithCorrectIdAndPassword_SuccessfullyDeleted()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath};
            service.CreateFolder(token, addFolderDto);
            FolderDto folder = service.GetById(token, 1);
            Assert.IsNotNull(folder);
            service.DeleteFolder(token, 1);
            var folders = service.GetAll(token);
            Assert.IsNotNull(folders);
            Assert.AreEqual(0, folders.Count());
        }
        [TestMethod]
        public void DeleteFolder_WithPassword_WithCorrectIdAndPassword_SuccessfullyDeleted()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            PasswordDto passwordDto = new() { Password = password };
            FolderDto folder = service.GetById(token, 1, passwordDto);
            Assert.IsNotNull(folder);
            service.DeleteFolder(token, 1, passwordDto);
            var folders = service.GetAll(token);

            Assert.IsNotNull(folders);
            Assert.AreEqual(0, folders.Count());
        }
        [TestMethod]
        public void DeleteFolder_WithPassword_WithCorrectIdAndWrongPassword_ThrowsException()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            string wrongPassword = "123";
            PasswordDto passwordDto = new() { Password = wrongPassword };

            Assert.ThrowsException<BadRequestException>(() => service.DeleteFolder(token, 1, passwordDto));
        }
        [TestMethod]
        public void DeleteFolder_WithPassword_WithCorrectIdAndWithoutPassword_ThrowsException()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);

            Assert.ThrowsException<BadRequestException>(() => service.DeleteFolder(token, 1));
        }
        [TestMethod]
        public void DeleteFolder_WithPassword_WithWrongId_ThrowsException()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);

            Assert.ThrowsException<UnauthorizedAccessException>(() => service.DeleteFolder(token2, 1));
        }
        [TestMethod]
        public void UpdateFolder_WithCorrectData_SuccessfullyUpdated()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath};
            service.CreateFolder(token, addFolderDto);
            string newName = "Update";
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = newName, IconPath = newIcon };
            service.UpdateFolder(token, 1, updateFolderDto);
            var folder = service.GetById(token, 1);
            Assert.IsNotNull(folder);
            Assert.AreEqual(newName, folder.Name);
            Assert.AreEqual(newIcon, folder.IconPath);
        }
        [TestMethod]
        public void UpdateFolder_WithPassword_WithCorrectData_SuccessfullyUpdated()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            string newName = "Update";
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = newName, IconPath = newIcon, PasswordToFolder = password };
            service.UpdateFolder(token, 1, updateFolderDto);
            PasswordDto passwordDto = new() { Password = password };
            var folder = service.GetById(token, 1, passwordDto);
            Assert.IsNotNull(folder);
            Assert.AreEqual(newName, folder.Name);
            Assert.AreEqual(newIcon, folder.IconPath);
        }
        [TestMethod]
        public void UpdateFolder_WithPassword_WithWrongPassword_ThrowsException()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            string wrongPassword = "123";
            string newName = "Update";
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = newName, IconPath = newIcon, PasswordToFolder = wrongPassword};
            Assert.ThrowsException<BadRequestException>(() => service.UpdateFolder(token, 1, updateFolderDto));
        }
        [TestMethod]
        public void UpdateFolder_WithWrongId_ThrowsException()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath};
            service.CreateFolder(token, addFolderDto);
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            string newName = "Update";
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = newName, IconPath = newIcon };
            Assert.ThrowsException<UnauthorizedAccessException>(() => service.UpdateFolder(token2, 1, updateFolderDto));
        }
        [TestMethod]
        public void UpdateFolder_WithTakenName_ThrowsException()
        {
            string token = SetUpGetToken();
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string name2 = "Folder2";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath };
            service.CreateFolder(token, addFolderDto);
            AddFolderDto addFolderDto2 = new AddFolderDto() { Name = name2, IconPath = iconPath };
            service.CreateFolder(token, addFolderDto2);
            string newIcon = "UpdateIcon";
            UpdateFolderDto updateFolderDto = new() { Name = name, IconPath = newIcon };
            var validationResult = updateFolderValidator.Validate(updateFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void LockFolder_WithCorrectData_SuccessfullyLocked()
        {
            // Arrange
            string token = SetUpGetToken();
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png"};
            service.CreateFolder(token, addFolderDto);

            ConfirmedPasswordDto confirmedPasswordDto = new ConfirmedPasswordDto() { Password = password, ConfirmedPassword = password };
            service.LockFolder(token, 1, confirmedPasswordDto);

            var folder = unitOfWork.Folders.GetById(1);
            Assert.IsNotNull(folder);
            Assert.IsNotNull(folder.PasswordHash);
        }

        [TestMethod]
        public void LockFolder_WithIncorrectPassword_ThrowsException()
        {
            string token = SetUpGetToken();
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png"};
            service.CreateFolder(token, addFolderDto);

            ConfirmedPasswordDto confirmedPasswordDto = new ConfirmedPasswordDto() { Password = "incorrectpassword", ConfirmedPassword = password };
            var validationResult = confirmedPasswordValidator.Validate(confirmedPasswordDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void LockFolder_OnAlreadyLockedFolder_ThrowsException()
        {
            string token = SetUpGetToken();
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png", Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            ConfirmedPasswordDto confirmedPasswordDto = new ConfirmedPasswordDto() { Password = password, ConfirmedPassword = password };

            Assert.ThrowsException<BadRequestException>(() => service.LockFolder(token, 1, confirmedPasswordDto));
        }

        [TestMethod]
        public void LockFolder_OnNotOwnedFolder_ThrowsException()
        {
            string token = SetUpGetToken();
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png", Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);

            ConfirmedPasswordDto confirmedPasswordDto = new ConfirmedPasswordDto() { Password = password, ConfirmedPassword = password };

            Assert.ThrowsException<UnauthorizedAccessException>(() => service.LockFolder(token2, 1, confirmedPasswordDto));
        }

        [TestMethod]
        public void UnlockFolder_WithCorrectData_SuccessfullyUnlocked()
        {
            string token = SetUpGetToken();
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png", Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            PasswordDto passwordDto = new PasswordDto() { Password = password };

            service.UnlockFolder(token, 1, passwordDto);

            var folder = unitOfWork.Folders.GetById(1);
            Assert.IsNotNull(folder);
            Assert.IsNull(folder.PasswordHash);
        }

        [TestMethod]
        public void UnlockFolder_WithWrongPassword_ThrowsException()
        {
            string token = SetUpGetToken();
            string password = "test123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png", Password = password, ConfirmedPassword = password };
            service.CreateFolder(token, addFolderDto);
            PasswordDto wrongPasswordDto = new PasswordDto() { Password = "wrongpassword" };

            Assert.ThrowsException<BadRequestException>(() => service.UnlockFolder(token, 1, wrongPasswordDto));
        }

        [TestMethod]
        public void UnlockFolder_OnAlreadyUnlockedFolder_ThrowsException()
        {
            string token = SetUpGetToken();
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            service.CreateFolder(token, addFolderDto);
            PasswordDto passwordDto = new PasswordDto() { Password = "test123" };

            Assert.ThrowsException<BadRequestException>(() => service.UnlockFolder(token, 1, passwordDto));
        }

        [TestMethod]
        public void UnlockFolder_WithWrongFolderId_ThrowsException()
        {
            string token = SetUpGetToken();
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            service.CreateFolder(token, addFolderDto);
            int wrongFolderId = 2;
            PasswordDto passwordDto = new PasswordDto() { Password = "test123" };

            Assert.ThrowsException<NotFoundException>(() => service.UnlockFolder(token, wrongFolderId, passwordDto));
        }

        [TestMethod]
        public void UnlockFolder_WithWrongUserId_ThrowsException()
        {
            string token = SetUpGetToken();
            AddFolderDto addFolderDto = new AddFolderDto() { Name = "TestFolder", IconPath = "icons/my.png" };
            service.CreateFolder(token, addFolderDto);
            PasswordDto passwordDto = new PasswordDto() { Password = "test123" };
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);

            Assert.ThrowsException<UnauthorizedAccessException>(() => service.UnlockFolder(token2, 1, passwordDto));
        }
    }
}
