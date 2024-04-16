using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.Services;
using System.Text;

namespace OCR_API.Controllers
{

    [Route("api/ocrError")]
    [ApiController]
    [Authorize]
    public class NoteWordErrorController : ControllerBase
    {
        private readonly INoteWordErrorService noteWordErrorService;

        public NoteWordErrorController(INoteWordErrorService noteWordErrorService)
        {
            this.noteWordErrorService = noteWordErrorService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetAll([FromQuery] string userId = null)
        {
            var errors = userId != null && int.TryParse(userId, out int id) ? noteWordErrorService.GetAllForUser(id) : noteWordErrorService.GetAll();

            return Ok(errors);
        }

        [HttpGet("{errorId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetById(int errorId)
        {
            var error = noteWordErrorService.GetById(errorId);
            return Ok(error);
        }

        [HttpDelete("{errorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteByIdAsync(int errorId)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            noteWordErrorService.DeleteById(accessToken, errorId);
            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAllAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            noteWordErrorService.DeleteAll(accessToken);
            return NoContent();
        }

        [HttpGet("csv")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DownloadErrorsAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            MemoryStream memoryStream = noteWordErrorService.DownloadErrors(accessToken);
            return File(memoryStream, "application/zip", "errors.zip");

        }

    }
}
