using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katatsuki.API
{
    public class TrackEvent
    {
        public Track Track { get; }
        public TrackEvent(Track track)
        {
            this.Track = track;
        }
    }
}
