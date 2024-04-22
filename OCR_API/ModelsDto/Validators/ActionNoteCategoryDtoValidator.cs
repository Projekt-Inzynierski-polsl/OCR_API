using FluentValidation;
using OCR_API.ModelsDto.NoteCategoriesDtos;

namespace OCR_API.ModelsDto.Validators
{
    public class ActionNoteCategoryDtoValidator : AbstractValidator<ActionNoteCategoryDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public ActionNoteCategoryDtoValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .NotEmpty()
                .Custom((value, context) =>
                {
                    var userEntity = unitOfWork.NoteCategories.GetAllByUser(unitOfWork.UserId);
                    bool nameInUse = userEntity.Where(u => u.Name == value).Count() > 1;
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });
        }
    }
}
