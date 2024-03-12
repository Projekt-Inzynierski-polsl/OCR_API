using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using OCR_API;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.MappingProfiles;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.Validators;
using OCR_API.Repositories;
using OCR_API.Services;

namespace UnitTests
{
    [TestClass]
    public class AccountServiceTests
    {
        private readonly DbContextOptions<SystemDbContext> _options;
        private readonly IMapper mapper;
        private readonly SystemDbContext dbContext;
        private readonly Repository<User> repository;
        private readonly PasswordHasher<User> passwordHasher;
        private readonly AccountService service;
        private readonly RegisterUserDtoValidator validator;
      public AccountServiceTests()
        {
            _options = new DbContextOptionsBuilder<SystemDbContext>()
                .UseInMemoryDatabase(databaseName: "userServiceTestDatabase")
                .Options;

            dbContext = new SystemDbContext(_options);
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UserMappingProfile>();
            });
            mapper = new Mapper(configurationProvider);

            repository = new Repository<User>(dbContext);
            passwordHasher = new PasswordHasher<User>();
            var authenticationSettings = new AuthenticationSettings
            {
                JwtKey = "TESTKEY_TESTKEY_TESTKEY_TESTKEY_TESTKEY",
                JwtIssuer = "ocr-system-api.azurewebsites.net",
                JwtExpireDays = 2
            };
            service = new AccountService(repository, passwordHasher, mapper, authenticationSettings);
            validator = new RegisterUserDtoValidator(repository);
        }

        [TestMethod]
        public void TryRegisterUserWithValidatedData()
        {
                RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nick = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
                var validationResult = validator.Validate(dto);
                Assert.AreEqual(true, validationResult.IsValid);

                service.RegisterUser(dto);
                var userInDatabase = repository.GetById(1);
                Assert.IsTrue(userInDatabase is not null);
                Assert.AreEqual("testUser@dto.pl", userInDatabase.Email);
                Assert.AreEqual("TestUser", userInDatabase.Nick);
                var result = passwordHasher.VerifyHashedPassword(userInDatabase, userInDatabase.PasswordHash, dto.Password);
                Assert.AreEqual(PasswordVerificationResult.Success, result);
        }

        [TestMethod]
        public void TryRegisterUserWithWrongPassword()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nick = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = validator.Validate(dto);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [TestMethod]
        public void TryRegisterUserWithTakenEmail()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nick = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            service.RegisterUser(dto);
            RegisterUserDto takenEmailDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nick = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = validator.Validate(takenEmailDto);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [TestMethod]
        public void TryRegisterUserWithInvalidEmail()
        {
            RegisterUserDto dto = new RegisterUserDto() { Email = "test", Nick = "TestUser", Password = "Test", ConfirmedPassword = "TestPassword" };
            var validationResult = validator.Validate(dto);
            Assert.AreEqual(false, validationResult.IsValid);
        }
    }
}