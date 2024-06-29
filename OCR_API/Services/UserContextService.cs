using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace OCR_API.Services
{
    public interface IUserContextService
    {
        int GetUserId { get; }
        ClaimsPrincipal User { get; set; }
        public Task<string?> GetJwtToken { get; }
    }

    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private ClaimsPrincipal user;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.HttpContext?.User;
        }

        public int GetUserId => int.Parse(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);

        public ClaimsPrincipal User
        {
            get => user;
            set => user = value;
        }

        public Task<string?> GetJwtToken => httpContextAccessor.HttpContext?.GetTokenAsync("Bearer", "access_token");
    }
}