using FluentValidation;

namespace OCR_API.ModelsDto.Validators
{
    public class PasswordDtoValidator : AbstractValidator<PasswordDto>
    {
        public PasswordDtoValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}