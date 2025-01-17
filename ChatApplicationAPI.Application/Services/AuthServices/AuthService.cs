﻿using ChatApplicationAPI.Application.Services.PasswordHash;
using ChatApplicationAPI.Application.Services.UserServices;
using ChatApplicationAPI.Domain.Entities.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApplicationAPI.Application.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _conf;
        private readonly IUserService _userService;
        private readonly IPasswordHashing _passwordHashing;

        public AuthService(IConfiguration conf, IUserService userService, IPasswordHashing passwordHashing)
        {
            _conf = conf;
            _userService = userService;
            _passwordHashing = passwordHashing;
        }

        public async Task<ResponseLogin> GenerateToken(RequestLogin user)
        {
            if (user == null)
            {
                return new ResponseLogin
                {
                    Token = "User Not Found",
                };
            }

            if (await UserExist(user))
            {
                var result = await _userService.GetByAny(x => x.PhoneNumber == user.PhoneNumber);

                var permissions = new List<int>();

                if (result.Role == "User")
                {
                    permissions = new List<int>() { 1, 2, 3, 4, 5, 6, 8, 9, 10, 11 };
                }
                else if (result.Role == "Admin")
                {
                    permissions = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
                }

                var jsonContent = JsonSerializer.Serialize(permissions);


                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Role, result.Role),
                    new Claim("PhoneNumber", user.PhoneNumber),
                    new Claim("Username", result.Username),
                    new Claim("Id", result.Id.ToString()),
                    new Claim("CreatedDate", DateTime.UtcNow.ToString()),
                    new Claim("Permissions", jsonContent),
                };

                return await GenerateTokenn(claims);
            }

            return new ResponseLogin
            {
                Token = "Un Authorize",
            };

        }

        public async Task<ResponseLogin> GenerateTokenn(IEnumerable<Claim> additionalClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conf["JWT:Secret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var exDate = Convert.ToInt32(_conf["JWT:ExpireDate"] ?? "1");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64)
            };

            if (additionalClaims?.Any() == true)
                claims.AddRange(additionalClaims);


            var token = new JwtSecurityToken(
                issuer: _conf["JWT:ValidIssuer"],
                audience: _conf["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(exDate),
                signingCredentials: credentials);

            return new ResponseLogin()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };


        }


        public async Task<bool> UserExist(RequestLogin user)
        {

            var result = await _userService.GetByAny(x => x.PhoneNumber == user.PhoneNumber);

            if (result == null) return false;

            if (user.PhoneNumber == result.PhoneNumber && _passwordHashing.Verify(result.Password, user.Password, result.Salt))
            {
                return true;
            }

            return false;
        }
    }
}
