using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json.Bson;
using OCR_API;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.MappingProfiles;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.Validators;
using OCR_API.Repositories;
using OCR_API.Seeders;
using OCR_API.Services;

namespace UnitTests
{
    [TestClass]
    public class AccountServiceTests
    {
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IAccountService service;
        private readonly IValidator<RegisterUserDto> registerValidator;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private IUnitOfWork unitOfWork;
        public AccountServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            passwordHasher = new PasswordHasher<User>();
            registerValidator = new RegisterUserDtoValidator(unitOfWork);
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            service = new AccountService(unitOfWork, passwordHasher, mapper, jwtTokenHelper);
            
        }

        [TestMethod]
        public void TryRegisterUserWithValidatedData()
        {
                RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
                var validationResult = registerValidator.Validate(dto);
                Assert.AreEqual(true, validationResult.IsValid);
                service.RegisterAccount(dto);
                var userInDatabase = service.UnitOfWork.Users.GetById(1);
                Assert.IsNotNull(userInDatabase);
                Assert.AreEqual("testUser@dto.pl", userInDatabase.Email);
                Assert.AreEqual("TestUser", userInDatabase.Nickname);
                var result = passwordHasher.VerifyHashedPassword(userInDatabase, userInDatabase.PasswordHash, dto.Password);
                Assert.AreEqual(PasswordVerificationResult.Success, result);
        }

        [TestMethod]
        public void TryRegisterUserWithWrongPassword()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(dto);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [TestMethod]
        public void TryRegisterUserWithTakenEmail()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(dto);
            RegisterUserDto takenEmailDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(takenEmailDto);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [TestMethod]
        public void TryRegisterUserWithTakenNicnkane()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "test@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(dto);
            RegisterUserDto takenEmailDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(takenEmailDto);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [TestMethod]
        public void TryRegisterUserWithInvalidEmail()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "test", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(dto);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [TestMethod]
        public void TestVerifyingUserLogPassesWithCorrectLogPasses()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "testUser@dto.pl";
            string password = "TestPassword";
            bool result = service.VerifyUserLogPasses(email, password);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestVerifyingUserLogPassesWithWrongEmail()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "test@dto.pl";
            string password = "TestPassword";
            bool result = service.VerifyUserLogPasses(email, password);
            Assert.AreEqual(false, result);
        }
        [TestMethod]
        public void TestVerifyingUserLogPassesWithWrongPassword()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "testUser@dto.pl";
            string password = "Test";
            bool result = service.VerifyUserLogPasses(email, password);
            Assert.AreEqual(false, result);
        }

        //[TestMethod]
        //public void TestUpdatingUserWithCorrectData()
        //{
        //    RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
        //    service.RegisterAccount(registerDto);
        //    UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update@dto.pl", Nickname = "Update", Password = "updatedPassword", RoleId = 1 };
        //    var validationResult = updateValidator.Validate(updateUserDto);
        //    Assert.AreEqual(true, validationResult.IsValid);

        //    service.UpdateUser(1, updateUserDto);
        //    User userInDatabase = service.UnitOfWork.Users.GetById(1);
        //    Assert.IsNotNull(userInDatabase);
        //    Assert.AreEqual(updateUserDto.Email, userInDatabase.Email);
        //    Assert.AreEqual(updateUserDto.Nickname, userInDatabase.Nickname);
        //    Assert.AreEqual(updateUserDto.RoleId, userInDatabase.RoleId);

        //    var result = passwordHasher.VerifyHashedPassword(userInDatabase, userInDatabase.PasswordHash, updateUserDto.Password);
        //    Assert.AreEqual(PasswordVerificationResult.Success, result);
        //}

        //[TestMethod]
        //public void TestValidatingUpdateUserDtoWithWrongEmail()
        //{
        //    UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update.pl", Nickname = "Update", Password = "updatedPassword", RoleId = 1 };
        //    var validationResult = updateValidator.Validate(updateUserDto);
        //    Assert.AreEqual(false, validationResult.IsValid);
        //}

        //[TestMethod]
        //public void TestValidatingUpdateUserDtoWithTakenEmail()
        //{
        //    RegisterUserDto registerDto = new RegisterUserDto() { Email = "update@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
        //    service.RegisterAccount(registerDto);
        //    UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update@dto.pl", Nickname = "Update", Password = "updatedPassword", RoleId = 1 };
        //    var validationResult = updateValidator.Validate(updateUserDto);
        //    Assert.AreEqual(false, validationResult.IsValid);
        //}

        //[TestMethod]
        //public void TestValidatingUpdateUserDtoWithTakenNickname()
        //{
        //    RegisterUserDto registerDto = new RegisterUserDto() { Email = "test@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
        //    service.RegisterAccount(registerDto);
        //    UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update@dto.pl", Nickname = "TestUser", Password = "updatedPassword", RoleId = 1 };
        //    var validationResult = updateValidator.Validate(updateUserDto);
        //    Assert.AreEqual(false, validationResult.IsValid);
        //}

        //[TestMethod]
        //public void TestValidatingUpdateUserDtoWithWrongRole()
        //{
        //    UpdateUserDto updateUserDto = new UpdateUserDto() { Email = "update@dto.pl", Nickname = "TestUser", Password = "updatedPassword", RoleId = 0 };
        //    var validationResult = updateValidator.Validate(updateUserDto);
        //    Assert.AreEqual(false, validationResult.IsValid);
        //}

        //[TestMethod]
        //public void TryLoginIntoRegisterUser()
        //{
        //    RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nick = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
        //    service.RegisterUser(registerDto);
        //    LoginUserDto loginDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "TestPasswrod" };

        //}
    }
}