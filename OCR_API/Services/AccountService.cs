using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Transactions;

namespace OCR_API.Services
{
    public interface IAccountService
    {
        void RegisterUser(RegisterUserDto registerUserDto);
    }
    public class AccountService : IAccountService
    {
        private readonly IRepository<User> userRepository;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IMapper mapper;

        public AccountService(IRepository<User> userRepository, IPasswordHasher<User> passwordHasher, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
        }

        public void RegisterUser(RegisterUserDto registerUserDto)
        {
            var newUser = mapper.Map<User>(registerUserDto);
            var hashedPassword = passwordHasher.HashPassword(newUser, registerUserDto.Password);
            newUser.PasswordHash = hashedPassword;
            AddUserTransaction addUserTransaction = new(userRepository, newUser);
            addUserTransaction.Execute();

        }
    }
}
