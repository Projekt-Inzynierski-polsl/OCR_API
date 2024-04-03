﻿using Microsoft.AspNetCore.Authentication;
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
        public async Task<ActionResult> GetAllUserNotesAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var folders = noteService.GetAll(accessToken);
            return Ok(folders);
        }

        [HttpGet("{noteId}")]
        public async Task<ActionResult> GetNoteByIdAsync(int noteId)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var folder = noteService.GetById(accessToken, noteId);
            return Ok(folder);
        }

        [HttpPost]
        public async Task<ActionResult> CreateNoteAcync([FromBody] AddNoteDto noteToAdd)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var noteId = noteService.CreateNote(accessToken, noteToAdd);
            return Created($"api/user/note/{noteId}", noteId);

        }

        [HttpDelete("{noteId}")]
        public async Task<ActionResult> DeleteNoteAsync(int noteId)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            noteService.DeleteNote(accessToken, noteId);
            return NoContent();
        }

        [HttpPut("{noteId}/update")]
        public async Task<ActionResult> UpdateNoteAsync(int noteId, [FromBody] UpdateNoteDto updateNoteDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            noteService.UpdateNote(accessToken, noteId, updateNoteDto);
            return Ok();
        }

        [HttpPut("{noteId}/folder")]
        public async Task<ActionResult> ChangeNoteFolderAsync(int noteId, [FromBody] ChangeNoteFolderDto changenNoteFolderDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            noteService.ChangeNoteFolder(accessToken, noteId, changenNoteFolderDto);
            return Ok();
        }

        [HttpPut("{noteId}/categories")]
        public async Task<ActionResult> UpdateNoteCategoriesAsync(int noteId, [FromBody] UpdateNoteCategoriesDto updateNoteCategoriesFolderDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            noteService.UpdateNoteCategories(accessToken, noteId, updateNoteCategoriesFolderDto);
            return Ok();
        }
    }
}