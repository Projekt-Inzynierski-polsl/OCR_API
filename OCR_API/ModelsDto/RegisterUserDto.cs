using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace OCR_API.ModelsDto
{
    public class RegisterUserDto
    {
        public string Email { get; set; }
        public string Nick {  get; set; }
        public string Password { get; set; }
        public string ConfirmedPassword { get; set; }
        public int RoleId { get; set; } = 2;
    }
}
