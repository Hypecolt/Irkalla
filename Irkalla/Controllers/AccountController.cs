using Irkalla.Entities;
using Irkalla.Entities.Models;
using Irkalla.Payloads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace Irkalla.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IConfiguration _config { get; }
        private readonly IrkallaContext _db;

        public AccountController(IrkallaContext db, IConfiguration configuration)
        {
            _config = configuration;
            _db = db;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterPayload registerPayload)
        {
            try
            {
                var existingEmail = _db.Users.Any(u => u.Email == registerPayload.Email);

                if (existingEmail)
                {
                    return BadRequest(new { status = true, message = "Email already in use." });
                }

                var userToCreate = new User
                {
                    FirstName = registerPayload.FirstName,
                    LastName = registerPayload.LastName,
                    Email = registerPayload.Email,
                    Gender = registerPayload.Gender,
                    PasswordHash = BC.HashPassword(registerPayload.Password),
                    Role = "BasicUser"
                };

                _db.Users.Add(userToCreate);

                _db.SaveChanges();
                return Ok(new { status = true, user = userToCreate });
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginPayload loginPayload)
        {
            try
            {
                var foundUser = _db.Users.SingleOrDefault(u => u.Email == loginPayload.Email);

                if (foundUser != null)
                {
                    if (BC.Verify(loginPayload.Password, foundUser.PasswordHash))  //if(foundUser.PasswordHash == BC.HashPassword(loginPayload.Password))
                    {
                        var tokenString = GenerateJSONWebToken(foundUser);

                        return Ok(new { status = true, token = tokenString, firstName = foundUser.FirstName });
                    }
                    return BadRequest(new { status = true, message = "Password does not match account username." });
                }
                else
                {
                    return BadRequest(new { status = true, message = "User does not exist." });
                }
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

        }

        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("Role", user.Role),
                new Claim("Id", user.Id.ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddDays(30),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
