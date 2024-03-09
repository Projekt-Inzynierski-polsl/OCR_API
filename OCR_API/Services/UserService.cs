using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Repositories;

namespace OCR_API.Services
{
    public interface IUserService
    {
        IEnumerable<UserDto> GetAll();

    }
    public class UserService : IUserService
    {
        private readonly IRepository<User> userRepository;

        public UserService(IRepository<User> userRepository)
        {
            this.userRepository = userRepository;
        }

        public IEnumerable<UserDto> GetAll()
        {
            return null;
        }
    }
}
