using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.MappingProfiles;
using OCR_API.Middleware;
using OCR_API.ModelsDto.Validators;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OCR_API;
using Microsoft.Extensions.Configuration;

namespace UnitTests
{
    internal class Helper
    {
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
        private static IServiceProvider Provider()
        {
            var services = new ServiceCollection();

            services.AddDbContext<SystemDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddScoped<ErrorHandlingMiddleware>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IRepository<Role>, Repository<Role>>();
            var authenticationSettings = Configuration.GetSection("Authentication").Get<AuthenticationSettings>();
            services.AddSingleton(authenticationSettings);

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddAutoMapper(typeof(UserMappingProfile));

            return services.BuildServiceProvider();
        }

        public static T GetRequiredService<T>()
        {
            var provider = Provider();

            return provider.GetRequiredService<T>();
        }

    }
}
