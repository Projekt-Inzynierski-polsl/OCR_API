﻿namespace OCR_API.ModelsDto
{
    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }

    }
}
