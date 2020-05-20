using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dto;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _auth;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthRepository auth, IConfiguration configuration)
        {
            _auth = auth;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userDto)
        {
            userDto.UserName = userDto.UserName.ToLower();
            if (await _auth.UserExists(userDto.UserName))
                return BadRequest("User already exists");

            var userToCreate = new User()
            {
                UserName = userDto.UserName
            };

            var createdUser = await _auth.Register(userToCreate, userDto.Password);
            return StatusCode(201);
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(UserForRegisterDto userDto)
        {
            //throw new Exception("Computer says no");
            // var loginUser = await _auth.Login(userDto.UserName, userDto.Password);
            var loginUser = new User()
            {
                Id = 10,
                UserName = "pramodkolte"
            };

            if (loginUser == null)
                return Unauthorized();


            var claims = new[]{
                    new Claim(ClaimTypes.NameIdentifier,loginUser.Id.ToString()),
                    new Claim(ClaimTypes.Name,loginUser.UserName)
                };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}