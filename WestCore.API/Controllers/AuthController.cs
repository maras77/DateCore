using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WestCore.API.Data;
using WestCore.API.Models;
using WestCore.API.DTOs;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace WestCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDTO userDTO)
        {
            userDTO.Username = userDTO.Username.ToLower();
            if (await _repo.UserExists(userDTO.Username))
                return BadRequest("Username already exists");

            var newUser = new User
            {
                Username = userDTO.Username,
            };

            var createdUser = await _repo.Register(newUser, userDTO.Password);
            return StatusCode(201);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDTO userDTO)
        {
            var userFromRepo = await _repo.Login(userDTO.Username, userDTO.Password);
            if (userFromRepo == null) return Unauthorized();

            // prepare token data
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config.GetSection("AppSettings:Token").Value
                ));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // create jwt token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return Ok(new { 
                token = tokenHandler.WriteToken(token)
                });
        }

    }
}
