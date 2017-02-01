using Katatsuki.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katatsuki
{
    public class KatatsukiContext
    {
        public ICollection<Track> Tracks { get; set; }
        public string Query { get; set; } 
    }
}
