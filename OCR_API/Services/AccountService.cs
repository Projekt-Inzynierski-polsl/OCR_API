using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OCR_API.Entities;
using OCR_API.Exceptions;
using OCR_API.ModelsDto;
using OCR_API.Repositories;
using OCR_API.Transactions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OCR_API.Services
{
    public interface IAccountService
    {
        IUnitOfWork UnitOfWork { get; }
        string RegisterUser(RegisterUserDto registerUserDto);
        string TryLoginUserAndGenerateJwt(LoginUserDto loginUserDto);
        bool VerifyUserLogPasses(string email, string password);
        string CreateJwtToken(User user);
        void UpdateUser(int userId, UpdateUserDto updateUserDto);
        string GetJwtTokenIfValid(int userId, string jwtToken);
        bool IsTokenValid(string jwtToken);
        JwtSecurityToken ReadJwtToken(string jwtToken);
        void Logout(int userId, string jwtToken);
        void DeleteAccount(int userId);
    }
    public class AccountService : IAccountService
    {
        public IUnitOfWork UnitOfWork { get; }
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IMapper mapper;
        private readonly AuthenticationSettings authenticationSettings;

        public AccountService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, IMapper mapper, AuthenticationSettings authenticationSettings)
        {
            UnitOfWork = unitOfWork;
            this.passwordHasher = passwordHasher;
            this.mapper = mapper;
            this.authenticationSettings = authenticationSettings;
        }

        public string RegisterUser(RegisterUserDto registerUserDto)
        {
            var newUser = mapper.Map<User>(registerUserDto);
            var hashedPassword = passwordHasher.HashPassword(newUser, registerUserDto.Password);
            newUser.PasswordHash = hashedPassword;
            AddUserTransaction addUserTransaction = new(UnitOfWork.Users, newUser);
            addUserTransaction.Execute();
            UnitOfWork.Commit();
            var token = CreateJwtToken(newUser);
            return token;

        }

        public string TryLoginUserAndGenerateJwt(LoginUserDto loginUserDto)
        {
            if(VerifyUserLogPasses(loginUserDto.Email, loginUserDto.Password))
            {
                var user = UnitOfWork.Users.Entity.Include(u => u.Role).FirstOrDefault(u => u.Email == loginUserDto.Email);
                var token = CreateJwtToken(user);
                return token;
            }
            else
            {
                throw new BadRequestException("Invalid email or password.");
            }
        }

        public bool VerifyUserLogPasses(string email, string password)
        {
            var user = UnitOfWork.Users.Entity.FirstOrDefault(u => u.Email == email);

            if (user is null)
            {
                return false;
            }
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public string CreateJwtToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nickname),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(authenticationSettings.JwtIssuer, authenticationSettings.JwtIssuer, claims, expires: expires, signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
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

        public string GetJwtTokenIfValid(int userId, string jwtToken)
        {
            var isTokenValid = IsTokenValid(jwtToken);
            if (!isTokenValid)
            {
                throw new BadRequestException("Token has already expired.");
            }
            var user = UnitOfWork.Users.GetById(userId);
            string token = CreateJwtToken(user);
            return token;
        }

        public bool IsTokenValid(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(jwtToken);
            return token.ValidTo > DateTime.UtcNow;
        }

        public JwtSecurityToken ReadJwtToken(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ReadJwtToken(jwtToken);
        }

        public void Logout(int userId, string jwtToken)
        {
            UnitOfWork.BlackListedTokens.Add(new BlackListToken() { Token = jwtToken, UserId = userId });
        }

        public void DeleteAccount(int userId)
        {
            UnitOfWork.Users.Remove(userId);
        }
    }
}

