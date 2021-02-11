using Irkalla.Entities;
using Irkalla.Entities.Models;
using Irkalla.Payloads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irkalla.Controllers
{
    [Authorize]
    public class PostController : ControllerBase
    {

        private readonly IrkallaContext _db;

        public PostController(IrkallaContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult ShowPosts(int pageSize, int pageNumber)
        {
            try
            {
                var currentUser = HttpContext.User;

                if (currentUser.HasClaim(claim => claim.Type == "Role"))
                {
                    var role = currentUser.Claims.FirstOrDefault(c => c.Type == "Role").Value;

                    if (role == "Admin" || role == "BasicUser")
                    {
                        var posts = _db.Posts.AsNoTracking()
                                                .Include(p => p.User)
                                                .Skip((pageNumber) * pageSize)
                                                .Take(pageSize)
                                                .Select(p => new
                                                {
                                                    PostId = p.Id,
                                                    PostText = p.Text,
                                                    PostImage = p.Image,
                                                    PostLikes = p.Likes,
                                                    OwnerFirstName = p.User.FirstName,
                                                    OwnerLastName = p.User.LastName,
                                                })
                                                .ToList();

                        return Ok(posts);
                    }

                    return BadRequest(new { status = true, message = "Phantom user." });

                }

                return BadRequest(new { status = true, message = "Nice try hackerperson." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = true, message = "N-ai voie." });
            }

        }

        [HttpPost]
        public ActionResult<Post> Create([FromBody]PostPayload payload)
        {
            try
            {
                var currentUser = HttpContext.User;

                var currUserId = int.Parse(currentUser.Claims.FirstOrDefault(c => c.Type == "Id").Value);

                var newPost = new Post
                {
                    UserId = currUserId,
                    User = _db.Users.Where(user => currUserId == user.Id).Single(),
                    Text = payload.Text,
                    Image = payload.Image,
                    dateTime = DateTime.UtcNow
                };

                _db.Posts.Add(newPost);
                _db.SaveChanges();

                return new JsonResult(new { status = true, message = "Post created successfully." });
            }
            catch (Exception)
            {

                return BadRequest(new { status = true, message = "Nu a mers sa postezi, n-ai drepturi." });
            }
        }
    }
}
