using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katatsuki.API
{
    public class TrackboxEventArgs
    {
        public Track Track { get; }
        public TrackboxEventArgs(Track track)
        {
            this.Track = track;
        }
    }
}
