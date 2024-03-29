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
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.Validators;
using OCR_API.Repositories;
using OCR_API.Seeders;
using OCR_API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        public void TryRegisterUserWithWrongPassword()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(dto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void TryRegisterUserWithTakenEmail()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(dto);
            RegisterUserDto takenEmailDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(takenEmailDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void TryRegisterUserWithTakenNicnkane()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "test@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(dto);
            RegisterUserDto takenEmailDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(takenEmailDto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void TryRegisterUserWithInvalidEmail()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "test", Nickname = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = registerValidator.Validate(dto);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void TestVerifyingUserLogPassesWithCorrectLogPasses()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "testUser@dto.pl";
            string password = "TestPassword";
            bool result = service.VerifyUserLogPasses(email, password, out User user);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestVerifyingUserLogPassesWithWrongEmail()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "test@dto.pl";
            string password = "TestPassword";
            bool result = service.VerifyUserLogPasses(email, password, out User user);
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void TestVerifyingUserLogPassesWithWrongPassword()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            string email = "testUser@dto.pl";
            string password = "Test";
            bool result = service.VerifyUserLogPasses(email, password, out User user);
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void TestLoginUserWithCorrectPassword()
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
        public void TestLoginUserWithWrongEmail()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "test@dto.pl", Password = "TestPassword" };
            Assert.ThrowsException<BadRequestException>(() => service.TryLoginUserAndGenerateJwt(loginUserDto));

        }
        [TestMethod]
        public void TestLoginUserWithWrongPassword()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "Password" };
            Assert.ThrowsException<BadRequestException>(() => service.TryLoginUserAndGenerateJwt(loginUserDto));
        }
        [TestMethod]
        public void TestLoginWithCorrectToken()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            service.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "TestPassword" };
            string token = service.TryLoginUserAndGenerateJwt(loginUserDto);
            string newToken = service.GetJwtTokenIfValid(token);
            Assert.IsTrue(jwtTokenHelper.IsTokenValid(newToken));
        }

        [TestMethod]
        public void TestLogoutAccount()
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