using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
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
    [TestClass]
    public class NoteServiceTests
    {
        private readonly IPasswordHasher<User> userPasswordHasher;
        private readonly INoteService service;
        private readonly IAccountService accountService;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private IUnitOfWork unitOfWork;

        public NoteServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            userPasswordHasher = new PasswordHasher<User>();
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            service = new NoteService(unitOfWork, mapper);
            accountService = new AccountService(unitOfWork, userPasswordHasher, mapper, jwtTokenHelper);
        }

        public string SetUpGetToken()
        {
            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser@dto.pl", Nickname = "TestUser", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser@dto.pl", Password = "TestPassword" };
            string token = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            return token;
        }
    }
}
