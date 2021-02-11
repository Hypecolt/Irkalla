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
        public ActionResult<List<Post>> ShowPosts(int pageSize, int pageNumber)
        {
            var currentUser = HttpContext.User;

            if (currentUser.HasClaim(claim => claim.Type == "Role"))
            {
                var role = currentUser.Claims.FirstOrDefault(c => c.Type == "Role").Value;

                if (role == "Admin" || role == "BasicUser")
                {
                    var postsQuery = _db.Posts.AsNoTracking();

                    postsQuery = postsQuery
                    .Skip((pageNumber) * pageSize)
                    .Take(pageSize);

                    var posts = postsQuery.ToList();

                    return new JsonResult(new { posts = posts, userName = _db.Users.ToList().Single(user => posts.Single().UserId == user.Id)});
                }

                return BadRequest(new { status = true, message = "Phantom user." });

            }

            return BadRequest(new { status = true, message = "Nice try hackerperson." });

        }

        [HttpPost]
        public ActionResult<Post> Create([FromBody]PostPayload payload)
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

            _db.Add(newPost);
            _db.SaveChanges();

            return new JsonResult(new { status = true, message = "Post created successfully." });
        }
    }
}
