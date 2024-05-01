using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCR_API.DbContexts;
using OCR_API.Entities;
using OCR_API.Exceptions;
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
        PageResults<UserDto> GetAll(GetAllQuery queryParameters);
        UserDto GetById(int id);
        UserDto GetLoggedUser(string accessToken);
        void UpdateUser(string jwtToken, int userId, UpdateUserDto updateUserDto);
        void DeleteUser(string jwtToken, int userId);
    }
    public class UserService : IUserService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;
        private readonly UserActionLogger logger;
        private readonly IPaginationService queryParametersService;

        public UserService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IMapper mapper, JwtTokenHelper jwtTokenHelper, UserActionLogger logger, IPaginationService queryParametersService)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
            this.logger = logger;
            this.queryParametersService = queryParametersService;
        }

        public PageResults<UserDto> GetAll(GetAllQuery queryParameters)
        {
            var spec = new UsersWithRoleSpecification(queryParameters.SearchPhrase);
            var usersQuery = UnitOfWork.Users.GetBySpecification(spec);
            var result = queryParametersService.PreparePaginationResults<UserDto, User>(queryParameters, usersQuery, mapper);

            return result;

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

        public void UpdateUser(string jwtToken, int userId, UpdateUserDto updateUserDto)
        {
            var adminId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var updatedUser = mapper.Map<User>(updateUserDto);
            if(updateUserDto.Password != null)
            {
                var hashedPassword = passwordHasher.HashPassword(updatedUser, updateUserDto.Password);
                updatedUser.PasswordHash = hashedPassword;
            }
            UpdateUserTransaction updateUserTransaction = new(UnitOfWork.Users, userId, updatedUser);
            updateUserTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.UpdateUser, adminId , DateTime.UtcNow, userId);
        }
        public void DeleteUser(string jwtToken, int userId)
        {
            var adminId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var user = UnitOfWork.Users.GetById(userId);
            if(user == null)
            {
                throw new NotFoundException("That user doesn't exist.");
            }
            DeleteEntityTransaction<User> deleteUserTransaction = new(UnitOfWork.Users, userId);
            deleteUserTransaction.Execute();
            UnitOfWork.Commit();
            logger.Log(EUserAction.DeleteUser, adminId, DateTime.UtcNow, userId);
        }

    }
}
