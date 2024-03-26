using FluentValidation;
using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.ModelsDto.Validators
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public RegisterUserDtoValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .Custom((value, context) =>
                {
                    bool emailInUse = unitOfWork.Users.Entity.Any(u => u.Email == value);
                    if(emailInUse)
                    {
                        context.AddFailure("Email", "That email is taken.");
                    }
                });

            RuleFor(x => x.Nickname)
                .MinimumLength(3)
                .Custom((value, context) =>
                {
                    bool nicknameInUse = unitOfWork.Users.Entity.Any(u => u.Nickname == value);
                    if (nicknameInUse)
                    {
                        context.AddFailure("Nickname", "That nickname is taken.");
                    }
                });

            RuleFor(x => x.Password)
                .MinimumLength(6);

            RuleFor(x => x.ConfirmedPassword)
                .Equal(e =>  e.Password);
        }
    }
}
