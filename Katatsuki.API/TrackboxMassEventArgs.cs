using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katatsuki.API
{
    public class TrackboxMassEventArgs
    {
        public IEnumerable<Track> Tracks { get; }
        public TrackboxMassEventArgs(IEnumerable<Track> track)
        {
            this.Tracks = track;
        }
    }
}
