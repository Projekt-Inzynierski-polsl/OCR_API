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
        public async Task<ActionResult> GetAllAsync([FromQuery]string? searchPhrase)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var categories = noteCategoriesService.GetAll(accessToken, searchPhrase);
            return Ok(categories);
        }

        [HttpGet("{categoryId}")]
        public async Task<ActionResult> GetByIdAsync(int categoryId)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var category = noteCategoriesService.GetById(accessToken, categoryId);
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult> AddNewCategory([FromBody] ActionNoteCategoryDto nameNoteCategoryDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            int id = noteCategoriesService.AddNewCategory(accessToken, nameNoteCategoryDto);
            return Created($"api/noteCategories/{id}", id);
        }

        [HttpDelete("{categoryId}")]
        public async Task<ActionResult> DeleteCategory(int categoryId)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            noteCategoriesService.DeleteCategory(accessToken, categoryId);
            return NoContent();
        }

        [HttpPut("{categoryId}")]
        public async Task<ActionResult> UpdateCategory(int categoryId, [FromBody] ActionNoteCategoryDto nameNoteCategoryDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            noteCategoriesService.UpdateCategory(accessToken, categoryId, nameNoteCategoryDto);
            return Ok();
        }
    }
}