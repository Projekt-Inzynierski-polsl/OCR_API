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
        public async Task<ActionResult> GetAllUserFoldersAsync([FromQuery]GetAllQuery queryParameters)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var folders = folderService.GetAll(accessToken, queryParameters);
            return Ok(folders);
        }

        [HttpPost("{folderId}")]
        public async Task<ActionResult> GetFolderByIdAsync(int folderId, [FromBody] PasswordDto? passwordDto = null)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var folder = folderService.GetById(accessToken, folderId, passwordDto);
            return Ok(folder);
        }

        [HttpPost]
        public async Task<ActionResult> CreateFolderAcync([FromBody] AddFolderDto folderToAdd)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var folderId = folderService.CreateFolder(accessToken, folderToAdd);
            return Created($"api/user/folder/{folderId}", folderId);
        }

        [HttpDelete("{folderId}")]
        public async Task<ActionResult> DeleteFolderAsync(int folderId, [FromBody] PasswordDto passwordDto = null)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            folderService.DeleteFolder(accessToken, folderId, passwordDto);
            return NoContent();
        }

        [HttpPut("{folderId}/update")]
        public async Task<ActionResult> UpdateFolderAsync(int folderId, [FromBody] UpdateFolderDto updateFolderDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            folderService.UpdateFolder(accessToken, folderId, updateFolderDto);
            return Ok();
        }

        [HttpPut("{folderId}/lock")]
        public async Task<ActionResult> LockFolderAsync(int folderId, [FromBody] ConfirmedPasswordDto confirmedPasswordDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            folderService.LockFolder(accessToken, folderId, confirmedPasswordDto);
            return Ok();
        }

        [HttpPut("{folderId}/unlock")]
        public async Task<ActionResult> UnlockFolderAsync(int folderId, [FromBody] PasswordDto passwordDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            folderService.UnlockFolder(accessToken, folderId, passwordDto);
            return Ok();
        }
    }
}
