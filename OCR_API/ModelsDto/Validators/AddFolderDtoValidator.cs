﻿using FluentValidation;

namespace OCR_API.ModelsDto.Validators
{
    public class AddFolderDtoValidator : AbstractValidator<AddFolderDto>
    {
        private readonly IUnitOfWork unitOfWork;

        public AddFolderDtoValidator(IUnitOfWork unitOfWork)
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

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotNull().When(x => x.Password != null)
                .MinimumLength(6).When(x => x.Password != null);

            RuleFor(x => x.ConfirmedPassword)
                .Equal(e => e.Password).When(x => x.Password != null);
        }
    }
}