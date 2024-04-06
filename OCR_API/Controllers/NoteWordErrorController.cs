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
        public ActionResult DeleteById(int errorId)
        {
            noteWordErrorService.DeleteById(errorId);
            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAll()
        {
            noteWordErrorService.DeleteAll();
            return NoContent();
        }

        [HttpGet("csv")]
        [Authorize(Roles = "Admin")]
        public ActionResult DownloadErrors()
        {
            MemoryStream memoryStream = noteWordErrorService.DownloadErrors();
            return File(memoryStream, "application/zip", "errors.zip");

        }

    }
}
