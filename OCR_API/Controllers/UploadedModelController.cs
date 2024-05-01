using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/uploadedModel")]
    [ApiController]
    [Authorize]
    public class UploadedModelController : ControllerBase
    {
        private readonly IUploadedModelService uploadedModelService;

        public UploadedModelController(IUploadedModelService uploadedModelService)
        {
            this.uploadedModelService = uploadedModelService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetAllUploadedModels()
        {
            var models = uploadedModelService.GetAll();
            return Ok(models);
        }

        [HttpGet("{modelId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetUploadedModelById(int modelId)
        {
            var model = uploadedModelService.GetById(modelId);
            return Ok(model);
        }

        [HttpGet("/user/{userId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetAllUploadedModelsByUserId(int userId)
        {
            var models = uploadedModelService.GetAllByUserId(userId);
            return Ok(models);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UploadNewModel()
        {
            var file = Request.Form.Files[0];
            uploadedModelService.UploadNewModelAsync(file);
            return Ok();
        }
    }
}
