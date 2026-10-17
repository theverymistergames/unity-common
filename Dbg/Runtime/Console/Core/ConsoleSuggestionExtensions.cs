using System;
using System.Collections.Generic;

namespace MisterGames.Dbg.Console.Core {

    public static class ConsoleSuggestionExtensions {

        /// <summary>
        /// Adds values that start with query first, then values that contain query, ignoring case.
        /// </summary>
        public static void AddMatches(this ICollection<string> suggestions, IReadOnlyList<string> values, string query) {
            query ??= string.Empty;

            for (int i = 0; i < values.Count; i++) {
                string value = values[i];
                if (!string.IsNullOrEmpty(value) && value.StartsWith(query, StringComparison.OrdinalIgnoreCase)) {
                    suggestions.Add(value);
                }
            }

            if (query.Length == 0) return;

            for (int i = 0; i < values.Count; i++) {
                string value = values[i];
                if (!string.IsNullOrEmpty(value) &&
                    !value.StartsWith(query, StringComparison.OrdinalIgnoreCase) &&
                    value.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    suggestions.Add(value);
                }
            }
        }
    }

}
