using Irkalla.Entities;
using Irkalla.Entities.Models;
using Irkalla.Payloads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Irkalla.Enums;
using BC = BCrypt.Net.BCrypt;

namespace Irkalla.Controllers
{
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IrkallaContext _db;

        public UserController(IrkallaContext db)
        {
            _db = db;
        }

        //user/searchusers?pageSize=10&pageNumber=sortType=0
        [HttpGet("allUsers")]
        public ActionResult<List<User>> ShowUsers(int pageSize, int pageNumber, UserSortType sortType)
        {
            var currentUser = HttpContext.User;

            if (currentUser.HasClaim(claim => claim.Type == "Role"))
            {
                var role = currentUser.Claims.FirstOrDefault(c => c.Type == "Role").Value;

                if (role == "Admin" || role == "BasicUser")
                {
                    var usersQuery = _db.Users.AsNoTracking();

                    switch (sortType)
                    {
                        case UserSortType.FirstNameAscendent: usersQuery = usersQuery.OrderBy(u => u.FirstName); break;
                        case UserSortType.FirstNameDescendent: usersQuery = usersQuery.OrderByDescending(u => u.FirstName); break;
                        case UserSortType.LasttNameAscendent: usersQuery = usersQuery.OrderBy(u => u.LastName); break;
                        case UserSortType.LastNameDescendent: usersQuery = usersQuery.OrderByDescending(u => u.LastName); break;
                        default: break;
                    }

                    usersQuery = usersQuery
                    .Skip((pageNumber) * pageSize)
                    .Take(pageSize);

                    var users = usersQuery.ToList();
                    
                    return users;
                }

                return BadRequest(new { status = true, message = "Phantom user." });

            }

            return BadRequest(new { status = true, message = "Nice try hackerperson." });

        }

        [HttpGet]
        public ActionResult<List<User>> GetAll()
        {
            var currentUser = HttpContext.User;

            if(currentUser.HasClaim(claim=>claim.Type == "Role"))
            {
                var role = currentUser.Claims.FirstOrDefault(c => c.Type == "Role").Value;

                if(role == "Admin" || role == "BasicUser") return _db.Users.ToList();
            }

            return BadRequest(new { status = true, message = "Nice try hackerperson." });
        }

        [HttpGet]
        public ActionResult<User> GetById(int id)
        {
            return _db.Users.Where(user=>id == user.Id).Single();
        }

        [HttpPost]
        public ActionResult<User> Create([FromBody] UserPayload payload)
        {
            try
            {
                var userToAdd = new User
                {
                    FirstName = payload.FirstName,
                    LastName = payload.LastName,
                    Email = payload.Email,
                    PasswordHash = BC.HashPassword(payload.Password),
                    Gender = payload.Gender
                };

                _db.Users.Add(userToAdd);
                _db.SaveChanges();

                return Ok(userToAdd);
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

        }

        [HttpPost]
        public ActionResult<User> Update([FromBody] UserPayload payload)
        {
            try
            {
                var currentUser = HttpContext.User;

                var currUserId = int.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == "Id").Value);
                var userToUpdate = _db.Users.SingleOrDefault(user => currUserId == user.Id);

                if (payload.FirstName != null) userToUpdate.FirstName = payload.FirstName;
                if (payload.LastName != null) userToUpdate.LastName = payload.LastName;
                if (payload.Email != null) userToUpdate.Email = payload.Email;
                if (payload.Password != null) userToUpdate.PasswordHash = BC.HashPassword(payload.Password);
                if (payload.ProfilePicture != null) userToUpdate.ProfilePicture = payload.ProfilePicture;
                if (payload.Gender != null) userToUpdate.Gender = payload.Gender;

                _db.SaveChanges();
                return Ok(userToUpdate);
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            try
            {
                var userToDelete = _db.Users.Single(user => id == user.Id);

                _db.Users.Remove(userToDelete);
                _db.SaveChanges();
                return Ok(new {status=true});
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
