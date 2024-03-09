using Microsoft.AspNetCore.Mvc;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/user")]
    [ApiController]

    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        public ActionResult GetAll()
        {
            return Ok(null);
        }
    }
}
