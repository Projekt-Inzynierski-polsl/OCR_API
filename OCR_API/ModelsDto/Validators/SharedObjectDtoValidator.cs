using FluentValidation;

namespace OCR_API.ModelsDto.Validators
{
    public class SharedObjectDtoValidator : AbstractValidator<SharedObjectDto>
    {
        public SharedObjectDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.ObjectId)
                .NotEmpty();

        }
    }
}
