using FluentValidation;

namespace OCR_API.ModelsDto.Validators
{
    public class UpdateFolderDtoValidator : AbstractValidator<UpdateFolderDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public UpdateFolderDtoValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .NotEmpty()
                .Custom((value, context) =>
                {
                    bool nameInUse = unitOfWork.Folders.Entity.Any(u => u.Name == value);
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });
        }
    }
}
