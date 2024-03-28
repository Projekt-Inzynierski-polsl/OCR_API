using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using OCR_API.ModelsDto;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/account")]
    [ApiController]
    [Authorize]
    public class AccountControler : ControllerBase
    {
        private readonly IAccountService accountService;

        public AccountControler(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public ActionResult RegisterAccount([FromBody] RegisterUserDto registerUserDto)
        {
            string token = accountService.RegisterAccount(registerUserDto);
            return Ok(token);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult Login([FromBody] LoginUserDto loginUserDto)
        {
            string token = accountService.TryLoginUserAndGenerateJwt(loginUserDto);
            return Ok(token);
        }

        [HttpGet("{userId}/token")]
        public async Task<ActionResult> IsTokenValidAsync(int userId) 
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            string token = accountService.GetJwtTokenIfValid(userId, accessToken);
            return Ok(token);
        }

        [HttpPost("{userId}/logout")]
        public async Task<ActionResult> LogoutAsync(int userId)
        {
            var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
            accountService.Logout(userId, accessToken);
            return Ok();
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAccount(int userId)
        {
            accountService.DeleteAccount(userId);
            return NoContent();
        }


    }
}
