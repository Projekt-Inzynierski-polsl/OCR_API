﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Transactions;

namespace OCR_API.Services
{
    public interface IUserService
    {
        IUnitOfWork UnitOfWork { get; }
        IEnumerable<UserDto> GetAll();
        UserDto GetById(int id);
        void UpdateUser(int userId, UpdateUserDto updateUserDto);
    }
    public class UserService : IUserService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IMapper mapper;

        public UserService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IMapper mapper)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
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

        public void UpdateUser(int userId, UpdateUserDto updateUserDto)
        {
            var updatedUser = mapper.Map<User>(updateUserDto);
            var hashedPassword = passwordHasher.HashPassword(updatedUser, updateUserDto.Password);
            updatedUser.PasswordHash = hashedPassword;
            UpdateUserTransaction updateUserTransaction = new(UnitOfWork.Users, userId, updatedUser);
            updateUserTransaction.Execute();
            UnitOfWork.Commit();
        }


    }
}
