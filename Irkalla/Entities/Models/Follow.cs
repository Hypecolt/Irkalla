using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irkalla.Entities.Models
{
    public class Follow
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public int ChildId { get; set; }

        public User Parent { get; set; }

        public User Child { get; set; }
    }
}
