using FluentValidation;

namespace OCR_API.ModelsDto.Validators
{
    public class AddNoteDtoValidator : AbstractValidator<AddNoteDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public AddNoteDtoValidator(IUnitOfWork unitOfWork)
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


            RuleFor(x => x.FolderId)
                 .Cascade(CascadeMode.Stop)
                 .NotNull().When(x => x.FolderId != null)
                 .Custom((value, context) =>
                 {
                     if(value != null)
                     {
                         var userEntity = unitOfWork.Folders.GetAllByUser(unitOfWork.UserId);
                         bool folderExist = userEntity.Any(u => u.Id == value);
                         if (!folderExist)
                         {
                             context.AddFailure("FolderId", "That folder doesn't exist.");
                         }
                     }
                 });

            RuleFor(x => x.NoteFileId)
                .NotEmpty()
                 .Custom((value, context) =>
                 {
                     var userEntity = unitOfWork.NoteFiles.GetAllByUser(unitOfWork.UserId);
                     bool fileExist = userEntity.Any(u => u.Id == value);
                     if (!fileExist)
                     {
                         context.AddFailure("NoteFileId", "That file doesn't exist.");
                     }
                 });

        }
    }
}
