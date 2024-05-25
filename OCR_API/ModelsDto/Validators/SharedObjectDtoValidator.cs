using FluentValidation;
using OCR_API.ModelsDto.SharedDtos;

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
