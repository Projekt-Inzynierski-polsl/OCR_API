using System.Reflection;

namespace OCR_API.Registrars.Extensions
{
    public static class MapperExtension
    {
        public static void AddMappersFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var mapperTypes = assembly.GetTypes().Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                type.Namespace != null &&
                type.Namespace.EndsWith(".MappingProfiles"));

            foreach (var mapperType in mapperTypes)
            {
                services.AddAutoMapper(mapperType);
            }
        }
    }
}