using FluentValidation;
using OCR_API.Services;

namespace OCR_API.ModelsDto.Validators
{
    public class UpdateFolderDtoValidator : AbstractValidator<UpdateFolderDto>
    {
        private readonly IUnitOfWork UnitOfWork;

        public UpdateFolderDtoValidator(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            UnitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotNull().When(x => x.Name != null)
                .Custom((value, context) =>
                {
                    var userEntity = UnitOfWork.Folders.GetAllByUser(userContextService.GetUserId);
                    bool nameInUse = userEntity.Exists(u => u.Name == value);
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });
        }
    }
}