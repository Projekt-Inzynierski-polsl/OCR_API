using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountControler : ControllerBase
    {
        private readonly IAccountService accountService;

        public AccountControler(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost("register")]
        public ActionResult RegisterUser([FromBody] RegisterUserDto registerUserDto)
        {
            accountService.RegisterUser(registerUserDto);
            return Ok();
        }
    }
}
