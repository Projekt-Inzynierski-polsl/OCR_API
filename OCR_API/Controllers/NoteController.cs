using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/user/note")]
    [ApiController]
    [Authorize]
    public class NoteController : ControllerBase
    {
        private readonly INoteService noteService;

        public NoteController(INoteService noteService)
        {
            this.noteService = noteService;
        }

        [HttpGet("")]
        public ActionResult GetAllUserNotes([FromQuery] GetAllQuery queryParameters)
        {
            var notes = noteService.GetAllByUser(queryParameters);
            return Ok(notes);
        }

        [HttpGet("{noteId}")]
        public ActionResult GetNoteById(int noteId)
        {
            var note = noteService.GetById(noteId);
            return Ok(note);
        }

        [HttpGet("lastEdited")]
        public ActionResult GetLastEditedNotes([FromQuery] int amount = 3)
        {
            var note = noteService.GetLastEdited(amount);
            return Ok(note);
        }

        [HttpPost]
        public ActionResult CreateNote([FromBody] AddNoteDto noteToAdd)
        {
            var noteId = noteService.CreateNote(noteToAdd);
            return Created($"api/user/note/{noteId}", noteId);

        }

        [HttpDelete("{noteId}")]
        public ActionResult DeleteNoteAsync(int noteId)
        {
            noteService.DeleteNote(noteId);
            return NoContent();
        }

        [HttpPut("{noteId}/update")]
        public ActionResult UpdateNote(int noteId, [FromBody] UpdateNoteDto updateNoteDto)
        {
            noteService.UpdateNote(noteId, updateNoteDto);
            return Ok();
        }

        [HttpPut("{noteId}/folder")]
        public ActionResult ChangeNoteFolderAsync(int noteId, [FromBody] ChangeNoteFolderDto changenNoteFolderDto)
        {
            noteService.ChangeNoteFolder(noteId, changenNoteFolderDto);
            return Ok();
        }

        [HttpPut("{noteId}/categories")]
        public ActionResult UpdateNoteCategories(int noteId, [FromBody] UpdateNoteCategoriesDto updateNoteCategoriesFolderDto)
        {
            noteService.UpdateNoteCategories(noteId, updateNoteCategoriesFolderDto);
            return Ok();
        }

        [HttpGet("{noteId}/pdf")]
        public ActionResult ExportPdfById(int noteId)
        {
            string link = noteService.ExportPdfById(noteId);
            var baseUri = $"{Request.Scheme}://{Request.Host}";
            var fileUrl = new Uri(new Uri(baseUri), link).AbsoluteUri;
            return Ok(fileUrl);
        }

        [HttpGet("{noteId}/docx")]
        public ActionResult ExportDocxById(int noteId)
        {
            string link = noteService.ExportDocxById(noteId);
            var baseUri = $"{Request.Scheme}://{Request.Host}";
            var fileUrl = new Uri(new Uri(baseUri), link).AbsoluteUri;
            return Ok(fileUrl);
        }
    }
}
