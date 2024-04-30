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
using OCR_API.Logger;
using OCR_API.Registrars;

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
        var dbContext = Helper.GetRequiredService<SystemDbContext>();
        var roles = Helper.GetRoles();
        dbContext.Roles.AddRange(roles);
        dbContext.SaveChanges();
       return new UnitOfWork(dbContext) { UserId = 1 };
    }

    public static IEnumerable<Role> GetRoles()
    {
        List<Role> roles = [new Role() { Name = "Admin" }, new Role() { Name = "User" }];
        return roles;
    }
}