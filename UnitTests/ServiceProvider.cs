using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.MappingProfiles;
using OCR_API.Middleware;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.Validators;
using OCR_API.Repositories;
using OCR_API.Seeders;
using OCR_API.Services;
using OCR_API;
using Microsoft.EntityFrameworkCore;

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

        // Dodaj pozostałe usługi
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
        // Dodaj wszystkie niezbędne usługi
        services.AddScoped<ErrorHandlingMiddleware>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
        services.AddSingleton<IValidator<UpdateUserDto>, UpdateUserDtoValidator>();
        services.AddSingleton<IRepository<User>, Repository<User>>();
        services.AddSingleton<IRepository<Role>, Repository<Role>>();
        var authenticationSettings = Configuration.GetSection("Authentication").Get<AuthenticationSettings>();
        services.AddSingleton(authenticationSettings);

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddAutoMapper(typeof(UserMappingProfile));
    }

    public static IUnitOfWork CreateUnitOfWork()
    {
        var dbContext = Helper.GetRequiredService<SystemDbContext>();
        var roles = Helper.GetRoles();
        dbContext.Roles.AddRange(roles);
        dbContext.SaveChanges();
       return new UnitOfWork(dbContext);
    }

    public static IEnumerable<Role> GetRoles()
    {
        List<Role> roles = [new Role() { Name = "Admin" }, new Role() { Name = "User" }];
        return roles;
    }
}