using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;
using OCR_API.Logger;
using OCR_API.Middleware;
using OCR_API.Seeders;
using System.Reflection;

namespace OCR_API.Registrars.Extensions
{
    public static class MainExtension
    {
        public static void AddMainRegistarsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            services.AddScoped<Entity>();
            services.AddScoped<ImageCryptographer>();
            services.AddScoped<Seeder>();
            services.AddScoped<JwtTokenHelper>();
            services.AddScoped<UserActionLogger>();
            services.AddScoped<ErrorHandlingMiddleware>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddHttpContextAccessor();

            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IPasswordHasher<Folder>, PasswordHasher<Folder>>();
        }
    }
}