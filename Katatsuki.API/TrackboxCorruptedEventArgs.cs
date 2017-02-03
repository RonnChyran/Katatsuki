using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katatsuki.API
{
    public class TrackboxCorruptedEventArgs
    {
        public string Path{ get; }
        public TrackboxCorruptedEventArgs(string path)
        {
            this.Path = path;
        }
    }
}
