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
        public async Task<ActionResult> GetAllFoldersByUserIdAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var folders = sharedService.GetAllFoldersByUserId(accessToken);
            return Ok(folders);
        }

        [HttpGet("notes")]
        public async Task<ActionResult> GetAllNotesByUserIdAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var notes = sharedService.GetAllNotesByUserId(accessToken);
            return Ok(notes);
        }

        [HttpPost("folder")]
        public async Task<ActionResult> ShareFolderAsync(SharedObjectDto sharedObjectDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            sharedService.ShareFolder(accessToken, sharedObjectDto);
            return Ok();
        }

        [HttpPost("note")]
        public async Task<ActionResult> ShareNoteAsync(SharedObjectDto sharedObjectDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            sharedService.ShareNote(accessToken, sharedObjectDto);
            return Ok();
        }

        [HttpDelete("folder")]
        public async Task<ActionResult> UnshareFolderAsync(SharedObjectDto sharedObjectDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            sharedService.UnshareFolder(accessToken, sharedObjectDto);
            return Ok();
        }

        [HttpDelete("note")]
        public async Task<ActionResult> UnshareNoteAsync(SharedObjectDto sharedObjectDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            sharedService.UnshareNote(accessToken, sharedObjectDto);
            return Ok();
        }
    }
}
