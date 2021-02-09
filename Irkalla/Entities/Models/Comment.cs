using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irkalla.Entities.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int uId { get; set; }

        public String? Msg { get; set; }

        public byte[]? Img { get; set; }
    }
}
