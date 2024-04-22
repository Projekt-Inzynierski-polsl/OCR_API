using FluentValidation;
using OCR_API.ModelsDto;

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
                    var userEntity = unitOfWork.Folders.GetAllByUser(unitOfWork.UserId);
                    bool nameInUse = userEntity.Where(u => u.Name == value).Count() > 1;
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });
        }
    }
}
