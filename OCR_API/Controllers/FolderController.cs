using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("{folderId}")]
        [Authorize(Roles = "Admin,User")]
        public ActionResult GetAllUserFolders(int folderId)
        {
            var folder = folderService.GetById(folderId);
            return Ok(folder);
        }
    }
}
