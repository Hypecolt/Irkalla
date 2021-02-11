using Irkalla.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irkalla.Payloads
{
    public class PostPayload
    {
        public string? Text { get; set; }

        public Picture? Image { get; set; }
    }
}
