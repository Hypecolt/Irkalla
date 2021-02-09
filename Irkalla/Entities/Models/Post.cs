using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irkalla.Entities.Models
{
    public class Post
    {
        public int Id { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }

        public string? Text { get; set; }

        public Picture? Image { get; set; }

        public DateTime dateTime { get; set; }

        public int Likes { get; set; }

        public List<Comment> Comment { get; set; }
    }
}
