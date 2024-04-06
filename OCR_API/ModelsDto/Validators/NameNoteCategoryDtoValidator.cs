using FluentValidation;
using OCR_API.ModelsDto.NoteCategoriesDtos;

namespace OCR_API.ModelsDto.Validators
{
    public class NameNoteCategoryDtoValidator : AbstractValidator<NameNoteCategoryDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public NameNoteCategoryDtoValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .NotEmpty()
                .Custom((value, context) =>
                {
                    var userEntity = unitOfWork.NoteCategories.GetAllByUser(unitOfWork.UserId);
                    bool nameInUse = userEntity.Any(u => u.Name == value);
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });
        }
    }
}
