using FluentValidation;
using OCR_API.Services;

namespace OCR_API.ModelsDto.Validators
{
    public class AddNoteDtoValidator : AbstractValidator<AddNoteDto>
    {
        private readonly IUnitOfWork UnitOfWork;

        public AddNoteDtoValidator(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            UnitOfWork = unitOfWork;

            RuleFor(x => x.Name)
                .NotEmpty()
                .Custom((value, context) =>
                {
                    var userEntity = UnitOfWork.Notes.GetAllByUser(userContextService.GetUserId);
                    bool nameInUse = userEntity.Exists(u => u.Name == value);
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
                         var userEntity = UnitOfWork.Folders.GetAllByUser(userContextService.GetUserId);
                         bool folderExist = userEntity.Exists(u => u.Id == value);
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
                     var userEntity = UnitOfWork.NoteFiles.GetAllByUser(userContextService.GetUserId);
                     bool fileExist = userEntity.Exists(u => u.Id == value);
                     if (!fileExist)
                     {
                         context.AddFailure("NoteFileId", "That file doesn't exist.");
                     }
                 });

        }
    }
}
