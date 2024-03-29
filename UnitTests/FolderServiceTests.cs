using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Services;
using OCR_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCR_API.ModelsDto.Validators;

namespace UnitTests
{
    [TestClass]
    public class FolderServiceTests
    {
        private readonly IPasswordHasher<Folder> folderPasswordHasher;
        private readonly IPasswordHasher<User> userPasswordHasher;
        private readonly IFolderService service;
        private readonly IAccountService accountService;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private IUnitOfWork unitOfWork;

        public FolderServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            folderPasswordHasher = new PasswordHasher<Folder>();
            userPasswordHasher = new PasswordHasher<User>();
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            service = new FolderService(unitOfWork, folderPasswordHasher, mapper, jwtTokenHelper);
            accountService = new AccountService(unitOfWork, userPasswordHasher, mapper, jwtTokenHelper);
        }
    }
}
