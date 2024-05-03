using FluentValidation;
using OCR_API.ModelsDto;
using OCR_API.Services;

namespace OCR_API.ModelsDto.Validators
{
    public class UpdateFolderDtoValidator : AbstractValidator<UpdateFolderDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public UpdateFolderDtoValidator(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            this.unitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotNull().When(x => x.Name != null)
                .Custom((value, context) =>
                {
                    var userEntity = unitOfWork.Folders.GetAllByUser(userContextService.GetUserId);
                    bool nameInUse = userEntity.Any(u => u.Name == value);
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });
        }
    }
}
