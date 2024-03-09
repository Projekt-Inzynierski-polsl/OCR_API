using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;
using OCR_API.Repositories;

namespace UnitTests
{
    [TestClass]
    public class UserServiceTests
    {
        private readonly DbContextOptions<SystemDbContext> _options;

        public UserServiceTests()
        {
            _options = new DbContextOptionsBuilder<SystemDbContext>()
                .UseInMemoryDatabase(databaseName: "userServiceTestDatabase")
                .Options;
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var context = new SystemDbContext(_options))
            {
                //var repository = new Repository<User>(context);
                //var service = new UserService(repository);
                //await service.AddUserAsync(user);
                //var userInDatabase = await context.Users.FindAsync(1);
                //Assert.NotNull(userInDatabase);
                //Assert.Equal("testuser", userInDatabase.Username);
            }
        }
    }
}