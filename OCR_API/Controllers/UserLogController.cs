﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.Services;

namespace OCR_API.Controllers
{
    [Route("api/userLog")]
    [ApiController]
    [Authorize]
    public class UserLogController : ControllerBase
    {
        private readonly UserLogService userLogService;

        public UserLogController(UserLogService userLogService)
        {
            this.userLogService = userLogService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Get([FromQuery]string type, [FromQuery] long startTimestamp = 0, [FromQuery] long endTimestamp = 0, [FromQuery] int userId = 0)
        {
            if (endTimestamp == 0)
            {
                endTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            var actions = userLogService.Get(type, startTimestamp, endTimestamp, userId);
            return Ok(actions);
        }

    }
}
