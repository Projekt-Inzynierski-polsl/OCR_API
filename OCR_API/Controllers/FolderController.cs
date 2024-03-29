using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/user/folder")]
    [ApiController]
    [Authorize]
    public class FolderController : ControllerBase
    {
        private readonly IFolderService folderService;

        public FolderController(IFolderService folderService)
        {
            this.folderService = folderService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> GetAllUserFoldersAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var folders = folderService.GetAll(accessToken);
            return Ok(folders);
        }

        [HttpPost("{folderId}")]
        [Authorize(Roles = "Admin,User")]
        public ActionResult GetFolderById(int folderId, [FromBody] PasswordDto? passwordDto)
        {
            var folder = folderService.GetById(folderId, passwordDto);
            return Ok(folder);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> CreateFolderAcync([FromBody] AddFolderDto folderToAdd)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var folderId = folderService.CreateFolder(accessToken, folderToAdd);
            return Created($"api/user/folder/{folderId}", folderId);
        }

        [HttpDelete("{folderId}")]
        [Authorize(Roles = "Admin,User")]
        public ActionResult DeleteAccount(int folderId)
        {
            folderService.DeleteFolder(folderId);
            return NoContent();
        }

    }
}
