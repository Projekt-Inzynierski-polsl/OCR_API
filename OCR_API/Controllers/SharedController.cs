using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/shared")]
    [ApiController]
    [Authorize]
    public class SharedController : ControllerBase
    {
        private readonly ISharedService sharedService;

        public SharedController(ISharedService sharedService)
        {
            this.sharedService = sharedService;
        }

        [HttpGet("folders")]
        public ActionResult GetAllFoldersByUserId()
        {
            var folders = sharedService.GetAllFoldersByUserId();
            return Ok(folders);
        }

        [HttpGet("notes")]
        public ActionResult GetAllNotesByUserId()
        {
            var notes = sharedService.GetAllNotesByUserId();
            return Ok(notes);
        }

        [HttpPost("folder")]
        public ActionResult ShareFolder(SharedObjectDto sharedObjectDto)
        {
            sharedService.ShareFolder(sharedObjectDto);
            return Ok();
        }

        [HttpPost("note")]
        public ActionResult ShareNote(SharedObjectDto sharedObjectDto)
        {
            sharedService.ShareNote(sharedObjectDto);
            return Ok();
        }

        [HttpDelete("folder")]
        public ActionResult UnshareFolder(SharedObjectDto sharedObjectDto)
        {
            sharedService.UnshareFolder(sharedObjectDto);
            return Ok();
        }

        [HttpDelete("note")]
        public ActionResult UnshareNote(SharedObjectDto sharedObjectDto)
        {
            sharedService.UnshareNote(sharedObjectDto);
            return Ok();
        }
    }
}
