using FluentValidation;
using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.ModelsDto.Validators
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        private readonly IRepository<User> userRepository;

        public RegisterUserDtoValidator(IRepository<User> userRepository)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .Custom((value, context) =>
                {
                    bool emailInUse = userRepository.Entity.Any(u => u.Email.ToString() == value);
                    if(emailInUse)
                    {
                        context.AddFailure("Email", "That email is taken.");
                    }
                });

            RuleFor(x => x.Nick)
                .MinimumLength(3);

            RuleFor(x => x.Password)
                .MinimumLength(6);

            RuleFor(x => x.ConfirmedPassword)
                .Equal(e =>  e.Password);
            this.userRepository = userRepository;
        }
    }
}
