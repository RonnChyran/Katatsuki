using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Katatsuki
{
    public class TrackQuery
    {
        public TrackQueryCombinator Combinator { get; }
        public string Predicate { get; }
        public string Parameter { get; }
        private static Regex FullQueryRegex = new Regex(@"((&&(\s+)?)?(\|\|(\s+)?)?![a-zA-Z]+[:]{.+?})", RegexOptions.Compiled);
        private static Regex QueryCommandRegex = new Regex(@"(?:!)(.+?)(?::)", RegexOptions.Compiled);
        private static Regex QueryParamRegex = new Regex(@"(?:{)(.+?)(?:})", RegexOptions.Compiled);
        public TrackQuery(TrackQueryCombinator combinator, string command, string parameter)
        {
            this.Combinator = combinator;
            this.Predicate = command;
            this.Parameter = parameter;
        }

        public static IEnumerable<TrackQuery> BuildQuerySet(string queryString)
        {
            var matches = TrackQuery.FullQueryRegex.Matches(queryString);
            foreach(var match in matches.Cast<Match>())
            {
                TrackQueryCombinator combinator = TrackQueryCombinator.NONE;
                if (match.Value.StartsWith("&&")) combinator = TrackQueryCombinator.AND;
                if (match.Value.StartsWith("||")) combinator = TrackQueryCombinator.OR;
                string command = TrackQuery.QueryCommandRegex.Match(match.Value)?.Groups[1]?.Value;
                if (command == null) yield break;
                string parameter = TrackQuery.QueryParamRegex.Match(match.Value)?.Groups[1]?.Value ?? "";
                yield return new TrackQuery(combinator, command, parameter);
            }
        }
    }
}
