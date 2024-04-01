﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.Repositories;
using OCR_API.Specifications;
using OCR_API.Transactions;
using OCR_API.Transactions.UserTransactions;
using OCR_API.ModelsDto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OCR_API.Services
{
    public interface IAccountService
    {
        IUnitOfWork UnitOfWork { get; }
        string RegisterAccount(RegisterUserDto registerUserDto);
        string TryLoginUserAndGenerateJwt(LoginUserDto loginUserDto);
        bool VerifyUserLogPasses(string email, string password, out User user);
        string GetJwtTokenIfValid(string jwtToken);
        void Logout(string jwtToken);
    }
    public class AccountService : IAccountService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IMapper mapper;
        private readonly JwtTokenHelper jwtTokenHelper;

        public AccountService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IMapper mapper, JwtTokenHelper jwtTokenHelper)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.jwtTokenHelper = jwtTokenHelper;
        }

        public string RegisterAccount(RegisterUserDto registerUserDto)
        {
            var newUser = mapper.Map<User>(registerUserDto);
            var role = UnitOfWork.Roles.GetById(newUser.RoleId);
            newUser.Role = role; 
            var hashedPassword = passwordHasher.HashPassword(newUser, registerUserDto.Password);
            newUser.PasswordHash = hashedPassword;
            AddUserTransaction addUserTransaction = new(UnitOfWork.Users, newUser);
            addUserTransaction.Execute();
            UnitOfWork.Commit();
            var token = jwtTokenHelper.CreateJwtToken(newUser);
            return token;

        }

        public string TryLoginUserAndGenerateJwt(LoginUserDto loginUserDto)
        {
            if(VerifyUserLogPasses(loginUserDto.Email, loginUserDto.Password, out User user))
            {
                var token = jwtTokenHelper.CreateJwtToken(user);
                return token;
            }
            else
            {
                throw new BadRequestException("Invalid email or password.");
            }
        }

        public bool VerifyUserLogPasses(string email, string password, out User user)
        {
            var spec = new UserByEmailWithRoleSpecification(email);
            user = UnitOfWork.Users.GetBySpecification(spec).FirstOrDefault();

            if (user is null)
            {
                return false;
            }
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }


        public string GetJwtTokenIfValid(string jwtToken)
        {
            var isTokenValid = jwtTokenHelper.IsTokenValid(jwtToken);
            if (!isTokenValid)
            {
                throw new BadRequestException("Token has already expired.");
            }
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            var user = UnitOfWork.Users.GetById(userId);
            string token = jwtTokenHelper.CreateJwtToken(user);
            return token;
        }

        public void Logout(string jwtToken)
        {
            var userId = jwtTokenHelper.GetUserIdFromToken(jwtToken);
            AddBlackListedTokenTransaction addBlackListedTokenTransaction = new(UnitOfWork.BlackListedTokens, userId, jwtToken);
            addBlackListedTokenTransaction.Execute();
            UnitOfWork.Commit();
        }
    }
}

