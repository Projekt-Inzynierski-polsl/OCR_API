﻿using AutoMapper;
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
using OCR_API.ModelsDto.NoteCategoriesDtos;
using Newtonsoft.Json.Linq;
using OCR_API.Exceptions;

namespace UnitTests
{
    [TestClass]
    public class NoteCategoryServiceTests
    {
        private readonly IPasswordHasher<User> userPasswordHasher;
        private readonly INoteCategoryService service;
        private readonly IAccountService accountService;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly IValidator<NameNoteCategoryDto> nameNoteCategoryValidator;
        private IUnitOfWork unitOfWork;

        public NoteCategoryServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            userPasswordHasher = new PasswordHasher<User>();
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            nameNoteCategoryValidator = new NameNoteCategoryDtoValidator(unitOfWork);
            service = new NoteCategoryService(unitOfWork, mapper, jwtTokenHelper);
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

        [TestMethod]
        public void GetAll_ReturnsCategoriesForUser()
        {

            var token = SetUpGetToken();
            var user1CategoryNames = new List<string> { "Category1", "Category2", "Category3" };
            foreach (var name in user1CategoryNames)
            {
                var addCategoryDto = new NameNoteCategoryDto() { Name = name };
                var category = service.AddNewCategory(token, addCategoryDto);
            }

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            var addCategoryDto2 = new NameNoteCategoryDto() { Name = "test" };
            var category2 = service.AddNewCategory(token2, addCategoryDto2);

            var user1Categories = service.GetAll(token);
            var user1CategoriesList = user1Categories.ToList();

            Assert.IsNotNull(user1Categories);
            Assert.AreEqual(3, user1Categories.Count());
            Assert.AreEqual(user1CategoryNames[0], user1CategoriesList[0].Name);
            Assert.AreEqual(user1CategoryNames[1], user1CategoriesList[1].Name);
            Assert.AreEqual(user1CategoryNames[2], user1CategoriesList[2].Name);

        }

        [TestMethod]
        public void AddNewCategory_ValidInput_CategorySuccessfullyAdded()
        {
            string token = SetUpGetToken();
            string categoryName = "TestCategory";
            NameNoteCategoryDto categoryDto = new NameNoteCategoryDto { Name = categoryName };

            int newCategoryId = service.AddNewCategory(token, categoryDto);

            Assert.IsTrue(newCategoryId > 0);
            var addedCategory = service.GetById(token, newCategoryId);
            Assert.IsNotNull(addedCategory);
            Assert.AreEqual(categoryName, addedCategory.Name);
        }

        [TestMethod]
        public void AddNewCategory_AddingDuplicateCategory_ValidationFails()
        {
            string token = SetUpGetToken();
            string categoryName = "TestCategory";
            NameNoteCategoryDto categoryDto1 = new NameNoteCategoryDto { Name = categoryName };
            NameNoteCategoryDto categoryDto2 = new NameNoteCategoryDto { Name = categoryName };

            int categoryId1 = service.AddNewCategory(token, categoryDto1);
            Assert.IsTrue(categoryId1 > 0);

            var validationResult = nameNoteCategoryValidator.Validate(categoryDto2);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void GetById_ValidCategoryId_ReturnsNoteCategoryDto()
        {
            string token = SetUpGetToken();
            var categoryDto = new NameNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(token, categoryDto);
            var retrievedCategoryDto = service.GetById(token, categoryId);

            Assert.IsNotNull(retrievedCategoryDto);
            Assert.AreEqual(categoryDto.Name, retrievedCategoryDto.Name);
        }

        [TestMethod]
        public void GetById_WithTokenFromAnotherUser_ThrowsUnauthorizedAccessException()
        {
            string token = SetUpGetToken();
            var categoryDto = new NameNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(token, categoryDto);

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);

            Assert.ThrowsException<UnauthorizedAccessException>(() => service.GetById(token2, categoryId));
        }

        [TestMethod]
        public void GetById_WithInvalidCategoryId_ReturnsNull()
        {
            string token = SetUpGetToken();
            var categoryDto = new NameNoteCategoryDto { Name = "TestCategory" };
            int validCategoryId = service.AddNewCategory(token, categoryDto);
            int invalidCategoryId = validCategoryId + 1;

            Assert.ThrowsException<NotFoundException>(() => service.GetById(token, invalidCategoryId));
        }

        [TestMethod]
        public void DeleteCategory_ValidCategoryId_CategorySuccessfullyDeleted()
        {
            string token = SetUpGetToken();
            var categoryDto = new NameNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(token, categoryDto);
            var categories = service.GetAll(token);
            Assert.AreEqual(1, categories.Count);

            service.DeleteCategory(token, categoryId);

            categories = service.GetAll(token);
            Assert.AreEqual(0, categories.Count);
        }

        [TestMethod]
        public void DeleteCategory_NonExistingCategoryId_ThrowsNotFoundException()
        {
            string token = SetUpGetToken();
            int invalidCategoryId = 999;

            Assert.ThrowsException<NotFoundException>(() => service.DeleteCategory(token, invalidCategoryId));
        }

        [TestMethod]
        public void DeleteCategory_CategoryNotBelongingToUser_ThrowsUnauthorizedAccessException()
        {
            string token = SetUpGetToken();
            var categoryDto = new NameNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(token, categoryDto);

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);

            Assert.ThrowsException<UnauthorizedAccessException>(() => service.DeleteCategory(token2, categoryId));
        }

        [TestMethod]
        public void UpdateCategoryName_ValidInput_CategoryNameSuccessfullyUpdated()
        {
            string token = SetUpGetToken();
            var categoryDto = new NameNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(token, categoryDto);

            string updatedName = "UpdatedCategoryName";
            var updatedCategoryDto = new NameNoteCategoryDto { Name = updatedName };
            service.UpdateCategoryName(token, categoryId, updatedCategoryDto);

            var retrievedCategory = service.GetById(token, categoryId);
            Assert.IsNotNull(retrievedCategory);
            Assert.AreEqual(updatedName, retrievedCategory.Name);
        }

        [TestMethod]
        public void UpdateCategoryName_ExistingName_ValidationFails()
        {
            string token = SetUpGetToken();
            var categoryDto1 = new NameNoteCategoryDto { Name = "TestCategory1" };
            var categoryDto2 = new NameNoteCategoryDto { Name = "TestCategory2" };
            int categoryId1 = service.AddNewCategory(token, categoryDto1);
            int categoryId2 = service.AddNewCategory(token, categoryDto2);

            var updatedCategoryDto2 = new NameNoteCategoryDto { Name = "TestCategory1" };
            var validationResult = nameNoteCategoryValidator.Validate(updatedCategoryDto2);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void UpdateCategoryName_NonExistingCategoryId_ThrowsNotFoundException()
        {
            string token = SetUpGetToken();
            int invalidCategoryId = 999;
            var updatedCategoryDto = new NameNoteCategoryDto { Name = "UpdatedCategoryName" };

            Assert.ThrowsException<NotFoundException>(() => service.UpdateCategoryName(token, invalidCategoryId, updatedCategoryDto));
        }

        [TestMethod]
        public void UpdateCategoryName_CategoryNotBelongingToUser_ThrowsUnauthorizedAccessException()
        {
            string token = SetUpGetToken();
            var categoryDto = new NameNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(token, categoryDto);

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            LoginUserDto loginUserDto = new LoginUserDto() { Email = "testUser2@dto.pl", Password = "TestPassword" };
            string token2 = accountService.TryLoginUserAndGenerateJwt(loginUserDto);

            var updatedCategoryDto = new NameNoteCategoryDto { Name = "UpdatedCategoryName" };
            Assert.ThrowsException<UnauthorizedAccessException>(() => service.UpdateCategoryName(token2, categoryId, updatedCategoryDto));
        }

    }
}