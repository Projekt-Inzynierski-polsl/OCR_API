using FluentValidation;

namespace OCR_API.ModelsDto.Validators
{
    public class ConfirmedPasswordDtoValidator : AbstractValidator<ConfirmedPasswordDto>
    {
        public ConfirmedPasswordDtoValidator()
        {
            RuleFor(x => x.Password)
                .MinimumLength(6);

            RuleFor(x => x.ConfirmedPassword)
                .Equal(e => e.Password);
        }
    }
}