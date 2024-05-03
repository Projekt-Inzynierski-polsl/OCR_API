using FluentValidation;
using OCR_API.Services;

namespace OCR_API.ModelsDto.Validators
{
    public class AddFolderDtoValidator : AbstractValidator<AddFolderDto>
    {
        private readonly IUnitOfWork UnitOfWork;

        public AddFolderDtoValidator(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            UnitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .NotEmpty()
                .Custom((value, context) =>
                {
                    var userEntity = UnitOfWork.Folders.GetAllByUser(userContextService.GetUserId);
                    bool nameInUse = userEntity.Exists(u => u.Name == value);
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotNull().When(x => x.Password != null)
                .MinimumLength(6).When(x => x.Password != null);

            RuleFor(x => x.ConfirmedPassword)
                .Equal(e => e.Password).When(x => x.Password != null);
        }
    }
}
