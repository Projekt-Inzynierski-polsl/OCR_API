using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.MappingProfiles;
using OCR_API.ModelsDto;
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

            service = new AccountService(repository, passwordHasher, mapper);
        }

        [TestMethod]
        public void TryRegisterUser()
        {
                RegisterUserDto dto = new RegisterUserDto() { Email = "testUser@dto.pl", Nick = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
                service.RegisterUser(dto);
                var userInDatabase = dbContext.Users.Find((uint)1);
                Assert.IsTrue(userInDatabase is not null);
                Assert.AreEqual("testUser@dto.pl", userInDatabase.Email);
        }
    }
}