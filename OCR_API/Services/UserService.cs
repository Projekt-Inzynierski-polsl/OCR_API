using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Logger;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.UserTransactions;

namespace OCR_API.Services
{
    public interface IUserService
    {
        IUnitOfWork UnitOfWork { get; }
        IEnumerable<UserDto> GetAll();
        UserDto GetById(int id);
        UserDto GetLoggedUser(string accessToken);
        void UpdateUser(int userId, UpdateUserDto updateUserDto);
        void DeleteUser(int userId);
    }
    public class UserService : IUserService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly UserActionLogger logger;

        public UserService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IMapper mapper, JwtTokenHelper jwtTokenHelper, UserActionLogger logger)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
            this.logger = logger;
        }

        public IEnumerable<UserDto> GetAll()
        {
            var usersDto = UnitOfWork.Users
                .GetAll()
                .Select(u => mapper.Map<UserDto>(u))
                .ToList();

            return usersDto;
        }
        public UserDto GetById(int id)
        {
            var user = UnitOfWork.Users.GetById(id);
            var userDto = mapper.Map<UserDto>(user);
            return userDto;
        }

        public UserDto GetLoggedUser(string jwtToken)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var spec = new UserByIdWithRoleSpecification(userId);
            var user = UnitOfWork.Users.GetBySpecification(spec).FirstOrDefault();
            var userDto = mapper.Map<UserDto>(user);
            return userDto;
        }

        public void UpdateUser(int userId, UpdateUserDto updateUserDto)
        {
            var updatedUser = mapper.Map<User>(updateUserDto);
            if(updateUserDto.Password != null)
            {
                var hashedPassword = passwordHasher.HashPassword(updatedUser, updateUserDto.Password);
                updatedUser.PasswordHash = hashedPassword;
            }
            UpdateUserTransaction updateUserTransaction = new(UnitOfWork.Users, userId, updatedUser);
            updateUserTransaction.Execute();
            UnitOfWork.Commit();
        }
        public void DeleteUser(int userId)
        {
            DeleteEntityTransaction<User> deleteUserTransaction = new(UnitOfWork.Users, userId);
            deleteUserTransaction.Execute();
            UnitOfWork.Commit();
        }

    }
}
