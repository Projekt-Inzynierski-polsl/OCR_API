using AutoMapper;
using Newtonsoft.Json.Linq;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.NoteCategoriesDtos;
using OCR_API.Specifications;
using OCR_API.Transactions.NoteCategoriesTransactions;
using OCR_API.Transactions.NoteTransactions;

namespace OCR_API.Services
{
    public interface INoteCategoryService
    {
        IUnitOfWork UnitOfWork { get; }
        ICollection<NoteCategoryDto> GetAll(string accessToken);
        NoteCategoryDto GetById(string accessToken, int categoryId);
        int AddNewCategory(string accessToken, NameNoteCategoryDto nameNoteCategoryDto);
        void DeleteCategory(string accessToken, int categoryId);
        void UpdateCategoryName(string accessToken, int categoryId, NameNoteCategoryDto nameNoteCategoryDto);

    }
    public class NoteCategoryService : INoteCategoryService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;


        public NoteCategoryService(IUnitOfWork unitOfWork, IMapper mapper, JwtTokenHelper jwtTokenHelper)
        {
            this.UnitOfWork = unitOfWork;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
        }

        public ICollection<NoteCategoryDto> GetAll(string jwtToken)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var spec = new NoteCategoriesByUserId(userId);
            var noteCategories = UnitOfWork.NoteCategories.GetBySpecification(spec);
            var noteCategoriesDto = noteCategories.Select(f => mapper.Map<NoteCategoryDto>(f)).ToList();

            return noteCategoriesDto;
        }

        public NoteCategoryDto GetById(string jwtToken, int categoryId)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            NoteCategory noteCategory = GetNoteCategoryIfBelongsToUser(userId, categoryId);
            var noteCategoryDto = mapper.Map<NoteCategoryDto>(noteCategory);

            return noteCategoryDto;
        }

        public int AddNewCategory(string jwtToken, NameNoteCategoryDto nameNoteCategoryDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            NoteCategory noteCategoryToAdd = mapper.Map<NoteCategory>(nameNoteCategoryDto);
            AddNoteCategoryTransaction addNoteCategoryTransaction = new(UnitOfWork, userId, noteCategoryToAdd.Name);
            addNoteCategoryTransaction.Execute();
            UnitOfWork.Commit();
            var newNoteCategoryId = addNoteCategoryTransaction.NoteCategoryToAdd.Id;
            return newNoteCategoryId;
        }

        public void DeleteCategory(string jwtToken, int categoryId)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            NoteCategory noteCategoryToRemove = GetNoteCategoryIfBelongsToUser(userId, categoryId);
            DeleteNoteCategoryTransaction deleteNoteCategoryTransaction = new(UnitOfWork.NoteCategories, noteCategoryToRemove.Id);
            deleteNoteCategoryTransaction.Execute();
            UnitOfWork.Commit();
        }

        public void UpdateCategoryName(string jwtToken, int categoryId, NameNoteCategoryDto nameNoteCategoryDto)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            NoteCategory noteCategoryToUpdate = GetNoteCategoryIfBelongsToUser(userId, categoryId);
            UpdateNoteCategoryTransaction updateNoteCategoryTransaction = new(noteCategoryToUpdate, nameNoteCategoryDto.Name);
            updateNoteCategoryTransaction.Execute();
            UnitOfWork.Commit();
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
