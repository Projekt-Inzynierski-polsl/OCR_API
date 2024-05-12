using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.ModelsDto.NoteFileDtos;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/noteFile")]
    [ApiController]
    [Authorize]
    public class NoteFileController : ControllerBase
    {
        private readonly INoteFileService noteFileService;

        public NoteFileController(INoteFileService ocrModelService)
        {
            this.noteFileService = ocrModelService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllByUser([FromQuery] GetAllQuery queryParameters)
        {
            var noteFiles = noteFileService.GetAllByUser(queryParameters);
            return Ok(noteFiles);
        }

        [HttpGet("{noteFileId}")]
        public async Task<ActionResult> GetById(int noteFileId)
        {
            var noteFile = noteFileService.GetById(noteFileId);
            return Ok(noteFile);
        }

        [HttpPost]
        public async Task<ActionResult> UploadFileAsync([FromForm] UploadFileDto uploadFileDto)
        {
            var file = await noteFileService.UploadFileAsync(uploadFileDto);
            return Created($"api/user/noteFile/{file.Id}", file);
        }

    }
}
