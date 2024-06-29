using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using OCR_API;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.NoteCategoriesDtos;
using OCR_API.ModelsDto.Validators;
using OCR_API.Services;

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
        private readonly IValidator<ActionNoteCategoryDto> actionNoteCategoryValidator;
        private readonly IUnitOfWork unitOfWork;
        private readonly UserActionLogger logger;
        private readonly PaginationService paginationService;
        private readonly IUserContextService userContextService;

        public NoteCategoryServiceTests()
        {
            unitOfWork = Helper.CreateUnitOfWork();
            userPasswordHasher = new PasswordHasher<User>();
            mapper = Helper.GetRequiredService<IMapper>();
            jwtTokenHelper = new JwtTokenHelper();
            userContextService = Helper.CreateMockIUserContextService();
            actionNoteCategoryValidator = new ActionNoteCategoryDtoValidator(unitOfWork, userContextService);
            logger = new UserActionLogger(unitOfWork);
            paginationService = new();
            service = new NoteCategoryService(unitOfWork, mapper, logger, paginationService, userContextService);
            accountService = new AccountService(unitOfWork, userPasswordHasher, mapper, jwtTokenHelper, logger, userContextService);
        }

        [TestMethod]
        public void GetAll_ReturnsCategoriesForUser()
        {
            var user1CategoryNames = new List<string> { "Category1", "Category2", "Category3" };
            foreach (var name in user1CategoryNames)
            {
                var addCategoryDto = new ActionNoteCategoryDto() { Name = name };
                service.AddNewCategory(addCategoryDto);
            }

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            var addCategoryDto2 = new ActionNoteCategoryDto() { Name = "test" };
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            service.AddNewCategory(addCategoryDto2);
            var parameters = new GetAllQuery() { PageNumber = 1, PageSize = 100 };
            Helper.ChangeIdInIUserContextService(userContextService, 1);
            var user1Categories = service.GetAllByUser(parameters);
            var user1CategoriesList = user1Categories.Items;

            Assert.IsNotNull(user1Categories);
            Assert.AreEqual(3, user1CategoriesList.Count);
            Assert.AreEqual(user1CategoryNames[0], user1CategoriesList[0].Name);
            Assert.AreEqual(user1CategoryNames[1], user1CategoriesList[1].Name);
            Assert.AreEqual(user1CategoryNames[2], user1CategoriesList[2].Name);
        }

        [TestMethod]
        public void AddNewCategory_ValidInput_CategorySuccessfullyAdded()
        {
            string categoryName = "TestCategory";
            ActionNoteCategoryDto categoryDto = new ActionNoteCategoryDto { Name = categoryName };

            int newCategoryId = service.AddNewCategory(categoryDto);

            Assert.IsTrue(newCategoryId > 0);
            var addedCategory = service.GetById(newCategoryId);
            Assert.IsNotNull(addedCategory);
            Assert.AreEqual(categoryName, addedCategory.Name);
        }

        [TestMethod]
        public void AddNewCategory_AddingDuplicateCategory_ValidationFails()
        {
            string categoryName = "TestCategory";
            ActionNoteCategoryDto categoryDto1 = new ActionNoteCategoryDto { Name = categoryName };
            ActionNoteCategoryDto categoryDto2 = new ActionNoteCategoryDto { Name = categoryName };

            int categoryId1 = service.AddNewCategory(categoryDto1);
            Assert.IsTrue(categoryId1 > 0);

            var validationResult = actionNoteCategoryValidator.Validate(categoryDto2);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void GetById_ValidCategoryId_ReturnsNoteCategoryDto()
        {
            var categoryDto = new ActionNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(categoryDto);
            var retrievedCategoryDto = service.GetById(categoryId);

            Assert.IsNotNull(retrievedCategoryDto);
            Assert.AreEqual(categoryDto.Name, retrievedCategoryDto.Name);
        }

        [TestMethod]
        public void GetById_WithTokenFromAnotherUser_ThrowsForbidException()
        {
            var categoryDto = new ActionNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(categoryDto);

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            Helper.ChangeIdInIUserContextService(userContextService, 2);

            Assert.ThrowsException<ForbidException>(() => service.GetById(categoryId));
        }

        [TestMethod]
        public void GetById_WithInvalidCategoryId_ReturnsNull()
        {
            var categoryDto = new ActionNoteCategoryDto { Name = "TestCategory" };
            int validCategoryId = service.AddNewCategory(categoryDto);
            int invalidCategoryId = validCategoryId + 1;

            Assert.ThrowsException<NotFoundException>(() => service.GetById(invalidCategoryId));
        }

        [TestMethod]
        public void DeleteCategory_ValidCategoryId_CategorySuccessfullyDeleted()
        {
            var categoryDto = new ActionNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(categoryDto);
            var parameters = new GetAllQuery() { PageNumber = 1, PageSize = 100 };
            var categories = service.GetAllByUser(parameters);
            Assert.AreEqual(1, categories.Items.Count);

            service.DeleteCategory(categoryId);

            categories = service.GetAllByUser(parameters);
            Assert.AreEqual(0, categories.Items.Count);
        }

        [TestMethod]
        public void DeleteCategory_NonExistingCategoryId_ThrowsNotFoundException()
        {
            Helper.RegisterAccount(accountService);
            int invalidCategoryId = 999;

            Assert.ThrowsException<NotFoundException>(() => service.DeleteCategory(invalidCategoryId));
        }

        [TestMethod]
        public void DeleteCategory_CategoryNotBelongingToUser_ThrowsForbidException()
        {
            Helper.RegisterAccount(accountService);
            var categoryDto = new ActionNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(categoryDto);

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);
            Helper.ChangeIdInIUserContextService(userContextService, 2);

            Assert.ThrowsException<ForbidException>(() => service.DeleteCategory(categoryId));
        }

        [TestMethod]
        public void UpdateCategoryName_ValidInput_CategoryNameSuccessfullyUpdated()
        {
            Helper.RegisterAccount(accountService);
            var categoryDto = new ActionNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(categoryDto);

            string updatedName = "UpdatedCategoryName";
            var updatedCategoryDto = new ActionNoteCategoryDto { Name = updatedName };
            service.UpdateCategory(categoryId, updatedCategoryDto);

            var retrievedCategory = service.GetById(categoryId);
            Assert.IsNotNull(retrievedCategory);
            Assert.AreEqual(updatedName, retrievedCategory.Name);
        }

        [TestMethod]
        public void UpdateCategoryName_ExistingName_ValidationFails()
        {
            Helper.RegisterAccount(accountService);
            var categoryDto1 = new ActionNoteCategoryDto { Name = "TestCategory1" };
            var categoryDto2 = new ActionNoteCategoryDto { Name = "TestCategory2" };
            service.AddNewCategory(categoryDto1);
            service.AddNewCategory(categoryDto2);

            var updatedCategoryDto2 = new ActionNoteCategoryDto { Name = "TestCategory1" };
            var validationResult = actionNoteCategoryValidator.Validate(updatedCategoryDto2);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void UpdateCategoryName_NonExistingCategoryId_ThrowsNotFoundException()
        {
            Helper.RegisterAccount(accountService);
            int invalidCategoryId = 999;
            var updatedCategoryDto = new ActionNoteCategoryDto { Name = "UpdatedCategoryName" };

            Assert.ThrowsException<NotFoundException>(() => service.UpdateCategory(invalidCategoryId, updatedCategoryDto));
        }

        [TestMethod]
        public void UpdateCategoryName_CategoryNotBelongingToUser_ThrowsForbidException()
        {
            Helper.RegisterAccount(accountService);
            var categoryDto = new ActionNoteCategoryDto { Name = "TestCategory" };
            int categoryId = service.AddNewCategory(categoryDto);

            RegisterUserDto registerDto = new RegisterUserDto() { Email = "testUser2@dto.pl", Nickname = "TestUser2", Password = "TestPassword", ConfirmedPassword = "TestPassword" };
            accountService.RegisterAccount(registerDto);

            var updatedCategoryDto = new ActionNoteCategoryDto { Name = "UpdatedCategoryName" };
            Helper.ChangeIdInIUserContextService(userContextService, 2);
            Assert.ThrowsException<ForbidException>(() => service.UpdateCategory(categoryId, updatedCategoryDto));
        }
    }
}