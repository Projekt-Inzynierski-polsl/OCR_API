using OCR_API.Entities;
using OCR_API.Registrars.Extensions;
using OCR_API.Repositories;
using OCR_API.Services;
using System.Reflection;

namespace OCR_API.Registrars
{
    public class Registar
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            AddServices(services, assembly);
        }

        public static void AddServices(IServiceCollection services, Assembly assembly)
        {
            services.AddServicesFromAssembly(assembly);
            services.AddRepositoriesFromAssembly(assembly);
            services.AddMappersFromAssembly(assembly);
            services.AddMainRegistarsFromAssembly(assembly);
        }
    }
}
