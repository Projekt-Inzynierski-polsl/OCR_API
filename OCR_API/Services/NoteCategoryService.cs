﻿using AutoMapper;
using Newtonsoft.Json.Linq;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.NoteCategoriesDtos;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.NoteCategoriesTransactions;
using OCR_API.Transactions.NoteTransactions;

namespace OCR_API.Services
{
    public interface INoteCategoryService
    {
        IUnitOfWork UnitOfWork { get; }
        PageResults<NoteCategoryDto> GetAllByUser(GetAllQuery queryParameters);
        NoteCategoryDto GetById(int categoryId);
        int AddNewCategory(ActionNoteCategoryDto actionNoteCategoryDto);
        void DeleteCategory(int categoryId);
        void UpdateCategory(int categoryId, ActionNoteCategoryDto actionNoteCategoryDto);

    }
    public class NoteCategoryService : INoteCategoryService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly UserActionLogger logger;
        private readonly IPaginationService queryParametersService;
        private readonly IUserContextService userContextService;

        public NoteCategoryService(IUnitOfWork unitOfWork, IMapper mapper, UserActionLogger logger, 
            IPaginationService queryParametersService, IUserContextService userContextService)
        {
            this.UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
            this.queryParametersService = queryParametersService;
            this.userContextService = userContextService;
        }

        public PageResults<NoteCategoryDto> GetAllByUser(GetAllQuery queryParameters)
        {
            var userId = userContextService.GetUserId;
            var spec = new NoteCategoriesByUserIdSpecification(userId, queryParameters.SearchPhrase);
            var noteCategoriesQuery = UnitOfWork.NoteCategories.GetBySpecification(spec);
            var result = queryParametersService.PreparePaginationResults<NoteCategoryDto, NoteCategory>(queryParameters, noteCategoriesQuery, mapper);

            return result;
        }

        public NoteCategoryDto GetById(int categoryId)
        {
            var userId = userContextService.GetUserId;
            NoteCategory noteCategory = GetNoteCategoryIfBelongsToUser(userId, categoryId);
            var noteCategoryDto = mapper.Map<NoteCategoryDto>(noteCategory);

            return noteCategoryDto;
        }

        public int AddNewCategory(ActionNoteCategoryDto actionNoteCategoryDto)
        {
            var userId = userContextService.GetUserId;
            NoteCategory noteCategoryToAdd = mapper.Map<NoteCategory>(actionNoteCategoryDto);
            AddNoteCategoryTransaction addNoteCategoryTransaction = new(UnitOfWork, userId, noteCategoryToAdd.Name, actionNoteCategoryDto.HexColor);
            addNoteCategoryTransaction.Execute();
            UnitOfWork.Commit();
            var newNoteCategoryId = addNoteCategoryTransaction.NoteCategoryToAdd.Id;
            logger.Log(EUserAction.AddCategory, userId, DateTime.UtcNow, newNoteCategoryId);
            return newNoteCategoryId;
        }

        public void DeleteCategory(int categoryId)
        {
            var userId = userContextService.GetUserId;
            NoteCategory noteCategoryToRemove = GetNoteCategoryIfBelongsToUser(userId, categoryId);
            DeleteEntityTransaction<NoteCategory> deleteNoteCategoryTransaction = new(UnitOfWork.NoteCategories, noteCategoryToRemove.Id);
            deleteNoteCategoryTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.DeleteCategory, userId, DateTime.UtcNow, categoryId);
        }

        public void UpdateCategory(int categoryId, ActionNoteCategoryDto actionNoteCategoryDto)
        {
            var userId = userContextService.GetUserId;
            NoteCategory noteCategoryToUpdate = GetNoteCategoryIfBelongsToUser(userId, categoryId);
            UpdateNoteCategoryTransaction updateNoteCategoryTransaction = new(noteCategoryToUpdate, actionNoteCategoryDto.Name, actionNoteCategoryDto.HexColor);
            updateNoteCategoryTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.UpdateCategory, userId, DateTime.UtcNow, categoryId);
        }

        private NoteCategory GetNoteCategoryIfBelongsToUser(int userId, int noteId)
        {
            var noteCategory = UnitOfWork.NoteCategories.GetById(noteId);
            if (noteCategory is null)
            {
                throw new NotFoundException("That entity doesn't exist.");
            }
            if (noteCategory.UserId != userId)
            {
                throw new ForbidException("Cannot operate someone else's note.");
            }
            return noteCategory;
        }
    }
}
