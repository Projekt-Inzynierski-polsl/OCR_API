using FluentValidation;
using OCR_API.Services;

namespace OCR_API.ModelsDto.Validators
{
    public class AddNoteDtoValidator : AbstractValidator<AddNoteDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public AddNoteDtoValidator(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            this.unitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .NotEmpty()
                .Custom((value, context) =>
                {
                    var userEntity = unitOfWork.Notes.GetAllByUser(userContextService.GetUserId);
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
                         var userEntity = unitOfWork.Folders.GetAllByUser(userContextService.GetUserId);
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
                     var userEntity = unitOfWork.NoteFiles.GetAllByUser(userContextService.GetUserId);
                     bool fileExist = userEntity.Any(u => u.Id == value);
                     if (!fileExist)
                     {
                         context.AddFailure("NoteFileId", "That file doesn't exist.");
                     }
                 });

        }
    }
}
