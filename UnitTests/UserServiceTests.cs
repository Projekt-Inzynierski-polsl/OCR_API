using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OCR_API.Entities;
using OCR_API.ModelsDto.Validators;
using OCR_API.ModelsDto;
using OCR_API.Services;
using OCR_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    internal class UserServiceTests
    {
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IUserService service;
        private readonly IValidator<RegisterUserDto> registerValidator;
        private readonly IValidator<UpdateUserDto> updateValidator;
        private readonly IMapper mapper;
        private IUnitOfWork unitOfWork;
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        public UserServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            passwordHasher = new PasswordHasher<User>();
            registerValidator = new RegisterUserDtoValidator(unitOfWork);
            mapper = Helper.GetRequiredService<IMapper>();

            service = new UserService(unitOfWork, passwordHasher, mapper);

        }
    }
}
