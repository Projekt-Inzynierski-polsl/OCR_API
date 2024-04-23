using AutoMapper;
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
        ICollection<NoteCategoryDto> GetAll(string accessToken, string? searchPhrase = null);
        NoteCategoryDto GetById(string accessToken, int categoryId);
        int AddNewCategory(string accessToken, ActionNoteCategoryDto actionNoteCategoryDto);
        void DeleteCategory(string accessToken, int categoryId);
        void UpdateCategory(string accessToken, int categoryId, ActionNoteCategoryDto acctionNoteCategoryDto);

    }
    public class NoteCategoryService : INoteCategoryService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly UserActionLogger logger;

        public NoteCategoryService(IUnitOfWork unitOfWork, IMapper mapper, JwtTokenHelper jwtTokenHelper, UserActionLogger logger)
        {
            this.UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
            this.logger = logger;
        }

        public ICollection<NoteCategoryDto> GetAll(string jwtToken, string? searchPhrase = null)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var noteCategories = UnitOfWork.NoteCategories.GetAllByUser(userId);
            var noteCategoriesDto = noteCategories.Where(f => searchPhrase == null || f.Name.Contains(searchPhrase, StringComparison.CurrentCultureIgnoreCase)).Select(f => mapper.Map<NoteCategoryDto>(f)).ToList();

            return noteCategoriesDto;
        }

        public NoteCategoryDto GetById(string jwtToken, int categoryId)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            NoteCategory noteCategory = GetNoteCategoryIfBelongsToUser(userId, categoryId);
            var noteCategoryDto = mapper.Map<NoteCategoryDto>(noteCategory);

            return noteCategoryDto;
        }

        public int AddNewCategory(string jwtToken, ActionNoteCategoryDto actionNoteCategoryDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            NoteCategory noteCategoryToAdd = mapper.Map<NoteCategory>(actionNoteCategoryDto);
            AddNoteCategoryTransaction addNoteCategoryTransaction = new(UnitOfWork, userId, noteCategoryToAdd.Name, actionNoteCategoryDto.HexColor);
            addNoteCategoryTransaction.Execute();
            UnitOfWork.Commit();
            var newNoteCategoryId = addNoteCategoryTransaction.NoteCategoryToAdd.Id;
            logger.Log(EUserAction.AddCategory, userId, DateTime.UtcNow, newNoteCategoryId);
            return newNoteCategoryId;
        }

        public void DeleteCategory(string jwtToken, int categoryId)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            NoteCategory noteCategoryToRemove = GetNoteCategoryIfBelongsToUser(userId, categoryId);
            DeleteEntityTransaction<NoteCategory> deleteNoteCategoryTransaction = new(UnitOfWork.NoteCategories, noteCategoryToRemove.Id);
            deleteNoteCategoryTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.DeleteCategory, userId, DateTime.UtcNow, categoryId);
        }

        public void UpdateCategory(string jwtToken, int categoryId, ActionNoteCategoryDto actionNoteCategoryDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
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
                throw new UnauthorizedAccessException("Cannot operate someone else's note.");
            }
            return noteCategory;
        }
    }
}
