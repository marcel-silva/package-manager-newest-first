using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NewestFirst
{
    // Pure policy: no credentials, networking, or Unity internals.
    public static class SortPolicy
    {
        public static string Rewrite(string url, bool reverse)
        {
            if (!reverse || !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                uri.AbsolutePath != "/-/api/purchases") return url;

            var queryStart = url.IndexOf('?');
            if (queryStart < 0) return url;
            var fragmentStart = url.IndexOf('#', queryStart);
            var queryEnd = fragmentStart < 0 ? url.Length : fragmentStart;
            var parts = url.Substring(queryStart + 1, queryEnd - queryStart - 1).Split(new[] { '&' });
            if (parts.Count(p => p.StartsWith("orderBy=", StringComparison.Ordinal)) != 1 ||
                parts.Count(p => p.StartsWith("order=", StringComparison.Ordinal)) != 1 ||
                !parts.Contains("orderBy=purchased_date") || !parts.Contains("order=desc")) return url;

            // Replace only this parameter, preserving offsets, filters and escaping exactly.
            var suffix = fragmentStart < 0 ? "" : url.Substring(fragmentStart);
            return url.Substring(0, queryStart + 1) +
                string.Join("&", parts.Select(p => p == "order=desc" ? "order=asc" : p)) + suffix;
        }

        // -1: newest first, +1: oldest first, 0: equal/insufficient/unreliable.
        public static int Direction(IEnumerable<DateTimeOffset> dates)
        {
            var values = dates.ToArray();
            var direction = 0;
            for (var i = 1; i < values.Length; i++)
            {
                var comparison = Math.Sign(values[i].CompareTo(values[i - 1]));
                if (comparison == 0) continue;
                if (direction != 0 && direction != comparison) return 0;
                direction = comparison;
            }
            return direction;
        }

        public static bool? Detect(IEnumerable<DateTimeOffset> asc, IEnumerable<DateTimeOffset> desc)
        {
            var ascending = Direction(asc);
            var descending = Direction(desc);
            if (ascending == -1 && descending == 1) return true;
            if (ascending == 1 && descending == -1) return false;
            return null;
        }

        internal static DateTimeOffset[] ReadDates(Dictionary<string, object> response)
        {
            if (!response.TryGetValue("results", out var value) ||
                !(value is System.Collections.IEnumerable rows)) return Array.Empty<DateTimeOffset>();
            var dates = new List<DateTimeOffset>();
            foreach (var row in rows)
            {
                if (!(row is Dictionary<string, object> fields) ||
                    !fields.TryGetValue("grantTime", out var date) ||
                    !DateTimeOffset.TryParse(Convert.ToString(date, CultureInfo.InvariantCulture),
                        CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
                    return Array.Empty<DateTimeOffset>();
                dates.Add(parsed);
            }
            return dates.ToArray();
        }
    }
}
