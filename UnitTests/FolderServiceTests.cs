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
        public void TestAddingNewFolderWithoutPassword()
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
        public void TestAddingNewFolderWithCorrectPassword()
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
        public void TestAddingNewFolderWithWrongConfirmedPassword()
        {
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "test123";
            string confirmedPassword = "123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = confirmedPassword };;
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void TestAddingNewFolderWithWrongPassword()
        {
            string name = "TestFolder";
            string iconPath = "icons/my.png";
            string password = "123";
            AddFolderDto addFolderDto = new AddFolderDto() { Name = name, IconPath = iconPath, Password = password, ConfirmedPassword = password }; ;
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void TestAddingNewFolderWithoutName()
        {
            string iconPath = "icons/my.png";
            string password = "123";
            AddFolderDto addFolderDto = new AddFolderDto() { IconPath = iconPath, Password = password, ConfirmedPassword = password }; ;
            var validationResult = addFolderValidator.Validate(addFolderDto);
            Assert.IsFalse(validationResult.IsValid);
        }
        [TestMethod]
        public void TestAddingNewFolderWithTakenName()
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
        public void TestGettingAllFolders()
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
    }
}
