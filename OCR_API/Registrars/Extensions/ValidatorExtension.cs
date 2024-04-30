using FluentValidation;
using OCR_API.Repositories;
using System.Reflection;

namespace OCR_API.Registrars.Extensions
{
    public static class ValidatorExtension
    {
        public static void AddValidatorsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var validatorTypes = assembly.GetTypes().Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                type.Namespace != null &&
                type.Namespace.EndsWith(".Validators"));

            foreach (var validatorType in validatorTypes)
            {
                var genericValidatorInterface = typeof(IValidator<>).MakeGenericType(validatorType);
                services.AddScoped(genericValidatorInterface, validatorType);
            }
        }
    }
}
