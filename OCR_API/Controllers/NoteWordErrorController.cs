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
        public ActionResult GetAll([FromQuery] GetAllQuery queryParameters)
        {
            var errors = noteWordErrorService.GetAll(queryParameters);

            return Ok(errors);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetAll(int userId, [FromQuery] GetAllQuery queryParameters)
        {
            var errors = noteWordErrorService.GetAllForUser(userId, queryParameters);

            return Ok(errors);
        }

        [HttpGet("{errorId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetById(int errorId)
        {
            var error = noteWordErrorService.GetById(errorId);
            return Ok(error);
        }

        [HttpGet("{errorId}/file")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetFileById(int errorId)
        {
            var file = await noteWordErrorService.GetFileById(errorId);
            return Ok(file);
        }

        [HttpPost]
        public async Task<ActionResult> ReportErrorAsync([FromBody] AddErrorDto addErrorDto)
        {
            var error = await noteWordErrorService.AddErrorAsync(addErrorDto);
            return Created($"api/ocrError/{error.Id}", error);
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
        public ActionResult DeleteAllAsync()
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

        [HttpPut("{errorId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult AcceptError(int errorId)
        {
            noteWordErrorService.AcceptError(errorId);
            return Ok();
        }

    }
}
