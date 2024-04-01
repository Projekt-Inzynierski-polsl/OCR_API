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
                    bool nameInUse = unitOfWork.Notes.Entity.Any(u => u.Name == value);
                    if (nameInUse)
                    {
                        context.AddFailure("Name", "That name is taken.");
                    }
                });


            RuleFor(x => x.FolderId)
                .NotEmpty()
                 .Custom((value, context) =>
                 {
                     bool folderExist = unitOfWork.Folders.Entity.Any(u => u.Id == value);
                     if (!folderExist)
                     {
                         context.AddFailure("FolderId", "That folder doesn't exist.");
                     }
                 });

            RuleFor(x => x.NoteFileId)
                .NotEmpty()
                 .Custom((value, context) =>
                 {
                     bool fileExist = unitOfWork.NoteFiles.Entity.Any(u => u.Id == value);
                     if (!fileExist)
                     {
                         context.AddFailure("NoteFileId", "That file doesn't exist.");
                     }
                 });

        }
    }
}
