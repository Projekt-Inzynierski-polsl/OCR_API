﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OCR_API.ModelsDto;
using OCR_API.Services;

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
        public ActionResult GetAll([FromQuery] GetAllQuery queryParameters)
        {
            var users = userService.GetAll(queryParameters);
            return Ok(users);
        }

        [HttpGet("{userId}")]
        public ActionResult GetById(int userId)
        {
            var user = userService.GetById(userId);
            return Ok(user);
        }

        [HttpGet("logged")]
        public ActionResult GetLoggedUserAsync()
        {
            var user = userService.GetLoggedUser();
            return Ok(user);
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateUser(int userId, [FromBody] UpdateUserDto updateUserDto)
        {
            userService.UpdateUser(userId, updateUserDto);
            return Ok();
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteUser(int userId)
        {
            userService.DeleteUser(userId);
            return NoContent();
        }
    }
}