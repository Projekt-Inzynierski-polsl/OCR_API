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
        public ActionResult GetAllUserFolders([FromQuery]GetAllQuery queryParameters)
        {
            var folders = folderService.GetAll(queryParameters);
            return Ok(folders);
        }

        [HttpPost("{folderId}")]
        public ActionResult GetFolderById(int folderId, [FromBody] PasswordDto? passwordDto = null)
        {
            var folder = folderService.GetById(folderId, passwordDto);
            return Ok(folder);
        }

        [HttpPost]
        public ActionResult CreateFolder([FromBody] AddFolderDto folderToAdd)
        {
            var folderId = folderService.CreateFolder(folderToAdd);
            return Created($"api/user/folder/{folderId}", folderId);
        }

        [HttpDelete("{folderId}")]
        public ActionResult DeleteFolder(int folderId, [FromBody] PasswordDto? passwordDto = null)
        {
            folderService.DeleteFolder(folderId, passwordDto);
            return NoContent();
        }

        [HttpPut("{folderId}/update")]
        public ActionResult UpdateFolder(int folderId, [FromBody] UpdateFolderDto updateFolderDto)
        {
            folderService.UpdateFolder(folderId, updateFolderDto);
            return Ok();
        }

        [HttpPut("{folderId}/lock")]
        public ActionResult LockFolder(int folderId, [FromBody] ConfirmedPasswordDto confirmedPasswordDto)
        {
            folderService.LockFolder(folderId, confirmedPasswordDto);
            return Ok();
        }

        [HttpPut("{folderId}/unlock")]
        public ActionResult UnlockFolder(int folderId, [FromBody] PasswordDto passwordDto)
        {
            folderService.UnlockFolder(folderId, passwordDto);
            return Ok();
        }
    }
}
