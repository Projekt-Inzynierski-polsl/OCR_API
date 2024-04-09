using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OCR_API.Entities;
using OCR_API.ModelsDto.Validators;
using OCR_API.ModelsDto;
using OCR_API.Services;
using OCR_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCR_API.Exceptions;
using OCR_API.Logger;

namespace UnitTests
{
    [TestClass]
    public class UserServiceTests
    {
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IUserService service;
        private readonly IAccountService accountService;
        private readonly IValidator<UpdateUserDto> updateValidator;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private IUnitOfWork unitOfWork;
        private readonly UserActionLogger logger;
        public UserServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            passwordHasher = new PasswordHasher<User>();
            updateValidator = new UpdateUserDtoValidator(unitOfWork);
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            logger = new UserActionLogger(unitOfWork.UserLogs);
            service = new UserService(unitOfWork, passwordHasher, mapper, jwtTokenHelper, logger);
            accountService = new AccountService(unitOfWork, passwordHasher, mapper, jwtTokenHelper, logger);

        }


        [TestMethod]
        public void GetAllUsers_ReturnsAllUsers()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            RegisterUserDto registerDto2 = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword2", ConfirmedPassword = "TestPassword2" };
            accountService.RegisterAccount(registerDto2);
            var users = service.GetAll();
            Assert.IsNotNull(users);
            Assert.AreEqual(2, users.Count());
            var firstUser = users.FirstOrDefault(u => u.Id == 1);
            Assert.IsNotNull(firstUser);
            var secondUser = users.FirstOrDefault(u => u.Id == 2);
            Assert.IsNotNull(secondUser);
            Assert.AreNotEqual(firstUser, secondUser);
            Assert.AreEqual("testUser@dto.pl", firstUser.Email);
            Assert.AreEqual("TestUser", firstUser.Nickname);
            Assert.AreEqual(2, firstUser.RoleId);

            Assert.AreEqual("testUser2@dto.pl", secondUser.Email);
            Assert.AreEqual("TestUser2", secondUser.Nickname);
            Assert.AreEqual(2, secondUser.RoleId);
        }

        [TestMethod]
        public void GetUserById_ReturnsUserById()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            RegisterUserDto registerDto2 = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword2", ConfirmedPassword = "TestPassword2" };
            accountService.RegisterAccount(registerDto2);
            var firstUser = service.GetById(1);
            Assert.IsNotNull(firstUser);
            var secondUser = service.GetById(2);
            Assert.IsNotNull(secondUser);
            Assert.AreNotEqual(firstUser, secondUser);
            Assert.AreEqual("testUser@dto.pl", firstUser.Email);
            Assert.AreEqual("TestUser", firstUser.Nickname);
            Assert.AreEqual(2, firstUser.RoleId);

            Assert.AreEqual("testUser2@dto.pl", secondUser.Email);
            Assert.AreEqual("TestUser2", secondUser.Nickname);
            Assert.AreEqual(2, secondUser.RoleId);
        }

        [TestMethod]
        public void DeleteUser_WithExistingId_SuccessfullyDeleted()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            var users = service.GetAll();
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count());
            service.DeleteUser(1);
            users = service.GetAll();
            Assert.IsNotNull(users);
            Assert.AreEqual(0, users.Count());
        }

        [TestMethod]
        public void DeleteUser_WithNotExistingId_ThrowsException()
        {
            Assert.ThrowsException<NotFoundException>(() => service.DeleteUser(1));
        }

        [TestMethod]
        public void UpdateUser_WithCorrectData_SuccessfullyUpdated()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update@dto.pl", Nickname = "Update", Password = "updatedPassword", RoleId = 1 };
            var validationResult = updateValidator.Validate(updateUserDto);
            Assert.IsTrue(validationResult.IsValid);

            service.UpdateUser(1, updateUserDto);
            User userInDatabase = service.UnitOfWork.Users.GetById(1);
            Assert.IsNotNull(userInDatabase);
            Assert.AreEqual(updateUserDto.Email, userInDatabase.Email);
            Assert.AreEqual(updateUserDto.Nickname, userInDatabase.Nickname);
            Assert.AreEqual(updateUserDto.RoleId, userInDatabase.RoleId);

            var result = passwordHasher.VerifyHashedPassword(userInDatabase, userInDatabase.PasswordHash, updateUserDto.Password);
            Assert.AreEqual(PasswordVerificationResult.Success, result);
        }

        [TestMethod]
        public void ValidateUpdateUserDto_WithWrongEmail_ValidationFails()
        {
            UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update.pl", Nickname = "Update", Password = "updatedPassword", RoleId = 1 };
            var validationResult = updateValidator.Validate(updateUserDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void ValidateUpdateUserDto_WithTakenEmail_ValidationFails()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "update@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update@dto.pl", Nickname = "Update", Password = "updatedPassword", RoleId = 1 };
            var validationResult = updateValidator.Validate(updateUserDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void ValidateUpdateUserDto_WithTakenNickname_ValidationFails()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "test@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update@dto.pl", Nickname = "TestUser", Password = "updatedPassword", RoleId = 1 };
            var validationResult = updateValidator.Validate(updateUserDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void ValidateUpdateUserDto_WithWrongRole_ValidationFails()
        {
            UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update@dto.pl", Nickname = "TestUser", Password = "updatedPassword", RoleId = 0 };
            var validationResult = updateValidator.Validate(updateUserDto);
            Assert.IsFalse(validationResult.IsValid);
        }
    }
}
