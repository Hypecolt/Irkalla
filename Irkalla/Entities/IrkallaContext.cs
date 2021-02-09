using Irkalla.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irkalla.Entities
{
    public class IrkallaContext : DbContext
    {
        public IrkallaContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Picture> Picutes { get; set; }
        public DbSet<Comment> Comments { get; set; }

    }
}
