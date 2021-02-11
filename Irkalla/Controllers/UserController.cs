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
                if (payload.Id.HasValue)
                {
                    var userToUpdate = _db.Users.SingleOrDefault(user => payload.Id.Value == user.Id);

                    userToUpdate.FirstName = payload.FirstName;
                    userToUpdate.LastName = payload.LastName;
                    userToUpdate.Email = payload.Email;
                    userToUpdate.PasswordHash = BC.HashPassword(payload.Password);
                    userToUpdate.ProfilePicture = payload.ProfilePicture;
                    userToUpdate.Gender = payload.Gender;

                    _db.SaveChanges();
                    return Ok(userToUpdate);
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
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
