using FluentValidation;

namespace OCR_API.ModelsDto.Validators
{
    public class UpdateNoteDtoValidator : AbstractValidator<UpdateNoteDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public UpdateNoteDtoValidator(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .NotEmpty()
                .Custom((value, context) =>
                {
                    var userEntity = unitOfWork.Notes.GetAllByUser(unitOfWork.UserId);
                    bool nameInUse = userEntity.Any(u => u.Name == value);
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });
        }
    }
}
