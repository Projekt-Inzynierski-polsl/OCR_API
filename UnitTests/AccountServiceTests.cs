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
using OCR_API.Exceptions;
using OCR_API.MappingProfiles;
using OCR_API.ModelsDto.Validators;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Seeders;
using OCR_API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using OCR_API.Logger;

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
        private UserActionLogger logger;
        public AccountServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            passwordHasher = new PasswordHasher<User>();
            registerValidator = new RegisterUserDtoValidator(unitOfWork);
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            logger = new UserActionLogger(unitOfWork.UserLogs);
            service = new AccountService(unitOfWork, passwordHasher, mapper, jwtTokenHelper, logger);
            
        }

        [TestMethod]
        public void RegisterUser_WithValidData_SuccessfullyRegistered()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(dto);
            Assert.IsTrue(validationResult.IsValid);
            service.RegisterAccount(dto);
            var userInDatabase = service.UnitOfWork.Users.GetById(1);
            Assert.IsNotNull(userInDatabase);
            Assert.AreEqual("testUser@dto.pl", userInDatabase.Email);
            Assert.AreEqual("TestUser", userInDatabase.Nickname);
            var result = passwordHasher.VerifyHashedPassword(userInDatabase, userInDatabase.PasswordHash, dto.Password);
            Assert.AreEqual(PasswordVerificationResult.Success, result);
        }

        [TestMethod]
        public void RegisterUser_WithWrongPassword_ValidationFails()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(dto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void RegisterUser_WithTakenEmail_ValidationFails()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(dto);
            RegisterUserDto takenEmailDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(takenEmailDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void RegisterUser_WithTakenNickname_ValidationFails()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "test@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(dto);
            RegisterUserDto takenEmailDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(takenEmailDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void RegisterUser_WithInvalidEmail_ValidationFails()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "test", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(dto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void VerifyUserLogPasses_WithCorrectLogPasses_ReturnsTrue()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "testUser@dto.pl";
            string password = "TestPassword";
            bool result = service.VerifyUserLogPasses(email, password, out User user);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void VerifyUserLogPasses_WithWrongEmail_ReturnsFalse()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "test@dto.pl";
            string password = "TestPassword";
            bool result = service.VerifyUserLogPasses(email, password, out User user);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void VerifyUserLogPasses_WithWrongPassword_ReturnsFalse()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "testUser@dto.pl";
            string password = "Test";
            bool result = service.VerifyUserLogPasses(email, password, out User user);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void LoginUser_WithCorrectPassword_ReturnsToken()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "TestPassword" };
            string token = service.TryLoginUserAndGenerateJwt(loginUserDto);
            Assert.IsNotNull(token);
            JwtSecurityToken jwtToken = jwtTokenHelper.ReadToken(token);
            Assert.IsNotNull(jwtToken);
            Assert.AreEqual(1, int.Parse(jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value));
            Assert.AreEqual("TestUser", jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value);
            Assert.AreEqual("User", jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value);
            Assert.IsTrue(jwtTokenHelper.IsTokenValid(token));
        }

        [TestMethod]
        public void LoginUser_WithWrongEmail_ThrowsException()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "test@dto.pl", Password = "TestPassword" };
            Assert.ThrowsException<BadRequestException>(() => service.TryLoginUserAndGenerateJwt(loginUserDto));
        }

        [TestMethod]
        public void LoginUser_WithWrongPassword_ThrowsException()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "Password" };
            Assert.ThrowsException<BadRequestException>(() => service.TryLoginUserAndGenerateJwt(loginUserDto));
        }

        [TestMethod]
        public void LoginWithCorrectToken_ReturnsValidToken()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "TestPassword" };
            string token = service.TryLoginUserAndGenerateJwt(loginUserDto);
            string newToken = service.GetJwtTokenIfValid(token);
            Assert.IsTrue(jwtTokenHelper.IsTokenValid(newToken));
        }

        [TestMethod]
        public void LogoutUser_LogsOutUser()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "TestPassword" };
            string token = service.TryLoginUserAndGenerateJwt(loginUserDto);
            service.Logout(token);
            BlackListToken expiredToken = unitOfWork.BlackListedTokens.GetById(1);
            Assert.IsNotNull(expiredToken);
            Assert.AreEqual(token, expiredToken.Token);
            Assert.AreEqual(1, expiredToken.UserId);
        }
    }
}