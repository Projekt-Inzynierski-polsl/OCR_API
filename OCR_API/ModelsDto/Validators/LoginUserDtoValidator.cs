﻿using FluentValidation;
using OCR_API.Entities;
using OCR_API.Repositories;

namespace OCR_API.ModelsDto.Validators
{
    public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
    {
        public LoginUserDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();

        }
    {
    }
}
