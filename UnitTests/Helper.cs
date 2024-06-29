using DotNetEnv;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OCR_API;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Registrars;
using OCR_API.Services;
using System.Security.Claims;

namespace UnitTests
{
    internal class Helper
    {
        private static IServiceProvider Provider()
        {
            Env.Load("../../../../OCR_API/.env");

            AuthenticationSettings.JwtKey = Environment.GetEnvironmentVariable("JwtKey");
            AuthenticationSettings.JwtExpireDays = int.Parse(Environment.GetEnvironmentVariable("JwtExpireDays"));
            AuthenticationSettings.JwtIssuer = Environment.GetEnvironmentVariable("JwtIssuer");
            var services = new ServiceCollection();

            services.AddDbContext<SystemDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            AddServices(services);

            return services.BuildServiceProvider();
        }

        public static T GetRequiredService<T>()
        {
            var provider = Provider();
            return provider.GetRequiredService<T>();
        }

        private static void AddServices(IServiceCollection services)
        {
            Registar registar = new Registar();
            registar.ConfigureServices(services);
        }

        public static IUnitOfWork CreateUnitOfWork()
        {
            var dbContext = GetRequiredService<SystemDbContext>();
            var roles = GetRoles();
            dbContext.Roles.AddRange(roles);
            dbContext.SaveChanges();
            return new UnitOfWork(dbContext);
        }

        public static IUserContextService CreateMockIUserContextService()
        {
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "1")
            }));

            httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(user);

            var userContextService = new UserContextService(httpContextAccessorMock.Object);

            return userContextService;
        }

        public static void ChangeIdInIUserContextService(IUserContextService userContextService, int userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }));

            userContextService.User = user;
        }

        public static IEnumerable<Role> GetRoles()
        {
            List<Role> roles = [new Role() { Name = "Admin" }, new Role() { Name = "User" }];
            return roles;
        }

        public static void RegisterAccount(IAccountService accountService, string email = "testUser@dto.pl", string nickname = "TestUser", string password = "TestPassword")
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = email, Nickname = nickname, Password = password, ConfirmedPassword = password };
            accountService.RegisterAccount(registerDto);
        }
    }
}