using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.NoteCategoriesDtos;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/noteCategories")]
    [ApiController]
    [Authorize]
    public class NoteCategoriesController : ControllerBase
    {
        private readonly INoteCategoryService noteCategoriesService;

        public NoteCategoriesController(INoteCategoryService noteCategoriesService)
        {
            this.noteCategoriesService = noteCategoriesService;
        }

        [HttpGet]
        public ActionResult GetAllByUser([FromQuery]GetAllQuery queryParameters)
        {
            var categories = noteCategoriesService.GetAllByUser(queryParameters);
            return Ok(categories);
        }

        [HttpGet("{categoryId}")]
        public ActionResult GetById(int categoryId)
        {
            var category = noteCategoriesService.GetById(categoryId);
            return Ok(category);
        }

        [HttpPost]
        public ActionResult AddNewCategory([FromBody] ActionNoteCategoryDto nameNoteCategoryDto)
        {
            int id = noteCategoriesService.AddNewCategory(nameNoteCategoryDto);
            return Created($"api/noteCategories/{id}", id);
        }

        [HttpDelete("{categoryId}")]
        public ActionResult DeleteCategory(int categoryId)
        {
            noteCategoriesService.DeleteCategory(categoryId);
            return NoContent();
        }

        [HttpPut("{categoryId}")]
        public ActionResult UpdateCategory(int categoryId, [FromBody] ActionNoteCategoryDto nameNoteCategoryDto)
        {
            noteCategoriesService.UpdateCategory(categoryId, nameNoteCategoryDto);
            return Ok();
        }
    }
}