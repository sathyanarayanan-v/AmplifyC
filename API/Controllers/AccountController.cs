using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController : BaseController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;

        }


        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register(AccountRegisterDto registerDto)
        {
            using var hmac = new HMACSHA512();

            if (await UserExists(registerDto.username)) return BadRequest("Oops. Username is taken");

            var user = new AppUser
            {
                UserName = registerDto.username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                PasswordSalt = hmac.Key,

            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.signToken(user);
            return new LoginResponseDto
            {
                username = user.UserName,
                token = token
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(user => user.UserName == loginDto.username);
            if (user == null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            var token = _tokenService.signToken(user);
            return new LoginResponseDto
            {
                username = user.UserName,
                token = token
            };

        }


        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(user => user.UserName.ToLower() == username.ToLower());
        }


    }
}