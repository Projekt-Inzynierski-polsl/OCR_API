using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.Services;
using OCR_API.ModelsDto;
using Microsoft.AspNetCore.Authentication;

namespace OCR_API.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetAll()
        {
            var users = userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{userId}")]

        public ActionResult GetById(int userId)
        {
            var user = userService.GetById(userId);
            return Ok(user);
        }

        [HttpGet("logged")]

        public async Task<ActionResult> GetLoggedUserAsync()
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            var user = userService.GetLoggedUser(accessToken);
            return Ok(user);
        }


        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateUserAsync(int userId, [FromBody] UpdateUserDto updateUserDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            userService.UpdateUser(accessToken, userId, updateUserDto);
            return Ok();
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAccountAsync(int userId)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            userService.DeleteUser(accessToken, userId);
            return NoContent();
        }
    }
}
