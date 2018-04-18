using Katatsuki.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katatsuki
{
    public class TrackQueryProcessor
    {
        private IDictionary<string, Func<Track, string, bool>> Predicates { get; }

        public TrackQueryProcessor(Func<Track, bool> duplicateFunction)
        {
            this.Predicates = new Dictionary<string, Func<Track, string, bool>>()
            {
                {"dup", (track, param) => duplicateFunction(track) },
                { "q", QueryFullText },
                {"Q", QueryExactText },
                {"a", (track, param) => track.Artist.Contains(param, StringComparison.InvariantCultureIgnoreCase) },
                {"A", (track, param) => track.Artist.Equals(param, StringComparison.InvariantCultureIgnoreCase)},
                {"al", (track, param) => track.Album.Contains(param, StringComparison.InvariantCultureIgnoreCase) },
                {"AL", (track, param) => track.Album.Equals(param, StringComparison.InvariantCultureIgnoreCase)},
                {"ala", (track, param) => (from artist in track.AlbumArtists where artist.Contains(param, StringComparison.InvariantCultureIgnoreCase) select artist).Any() },
                { "ALA", (track, param) => (from artist in track.AlbumArtists where artist.Equals(param, StringComparison.InvariantCultureIgnoreCase) select artist).Any() },
                { "f", (track, param) => {
                if (param.Equals("mp3", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.MP3_CBR
                                    || track.FileType == TrackFileType.MP3_VBR;
                            if (param.Equals("flac", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC
                                    || track.FileType == TrackFileType.FLAC_4
                                    || track.FileType == TrackFileType.FLAC_8
                                    || track.FileType == TrackFileType.FLAC_16
                                    || track.FileType == TrackFileType.FLAC_24
                                    || track.FileType == TrackFileType.FLAC_32;
                            if (param.Equals("alac", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.ALAC;
                            if(param.Equals("aac", StringComparison.InvariantCultureIgnoreCase)
                                || param.Equals("mp4", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.AAC;
                            if (param.Equals("flac4", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_4;
                            if (param.Equals("flac8", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_8;
                            if (param.Equals("flac16", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_16;
                            if (param.Equals("flac24", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_24;
                            if (param.Equals("flac32", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.FLAC_32;
                            if (param.Equals("cbr", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.MP3_CBR;
                            if(param.Equals("vbr", StringComparison.InvariantCultureIgnoreCase))
                                return track.FileType == TrackFileType.MP3_VBR;
                            return track.FileType == TrackFileType.UNKNOWN;} },
                {"sr", (track, param) =>
                {
                    Int32.TryParse(param, out int i);
                    return track.SampleRate.Equals(i);
                }},
                {"br", (track, param) =>
                {
                    Int32.TryParse(param, out int i);
                    return track.SampleRate.Equals(i);
                }},
                {"brlt", (track, param) =>
                {
                    Int32.TryParse(param, out int i);
                    return track.Bitrate < i;
                }},
                {"brgt", (track, param) =>
                {
                    Int32.TryParse(param, out int i);
                    return track.Bitrate > i;
                }},
                {"chlt", (track, param) =>
                {
                    Int32.TryParse(param, out int i);
                    return track.FrontCoverHeight <= i;
                }},
                {"chgt", (track, param) =>
                {
                    Int32.TryParse(param, out int i);
                    return track.FrontCoverHeight >= i;
                }},
                {"cwlt", (track, param) =>
                {
                    Int32.TryParse(param, out int i);
                    return track.FrontCoverWidth <= i;
                }},
                {"cwgt", (track, param) =>
                {
                    Int32.TryParse(param, out int i);
                    return track.FrontCoverWidth >= i;
                }},
                {"c", (track, param) =>
                {
                      Boolean.TryParse(param, out bool b);
                      return track.HasFrontCover == b;
                }},
                {"mb", (track, param) =>
                {
                      Boolean.TryParse(param, out bool b);
                      return b == (track.MusicBrainzTrackId != null && track.MusicBrainzTrackId != String.Empty);
                }},
            };
        }

        public bool GetFilterResult(Track track, TrackQuery query)
        {
            if (!this.Predicates.ContainsKey(query.Predicate)) return false;
            return this.Predicates[query.Predicate](track, query.Parameter);
        }

        public bool ProcessQuery(Track track, IEnumerable<TrackQuery> queries)
        {
            bool match = false;
            foreach(var query in queries)
            {
                bool queryMatch = this.GetFilterResult(track, query);
                switch(query.Combinator)
                {
                    case TrackQueryCombinator.NONE:
                        match = queryMatch;
                        break;
                    case TrackQueryCombinator.AND:
                        match = match && queryMatch;
                        break;
                    case TrackQueryCombinator.OR:
                        match = match || queryMatch;
                        break;
                }
            }
            return match;
        }

        public bool QueryFullText(Track track, string query)
        {
            return (track.Title.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                    || track.Album.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                    || track.Artist.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                    || (from artist in track.AlbumArtists where artist.Contains(query, StringComparison.InvariantCultureIgnoreCase) select artist).Any());
        }

        public bool QueryExactText(Track track, string exactQuery)
        {
            return (track.Title.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || track.Album.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || track.Artist.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase)
                   || (from artist in track.AlbumArtists where artist.Equals(exactQuery, StringComparison.InvariantCultureIgnoreCase) select artist).Any());
        }
    }
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
