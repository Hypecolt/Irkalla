using Irkalla.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irkalla.Payloads
{
    public class PostPayload
    {
        public int? Id { get; set; }

        public string? Text { get; set; }

        public string? Image { get; set; }
    }
}
