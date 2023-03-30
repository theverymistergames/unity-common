using System.Text.RegularExpressions;
using UnityEngine;

namespace MisterGames.Common.Strings {

    public static class StringUtils {

        public const string PatternFieldName = @"^([a-zA-Z_])([a-zA-Z0-9_]*)";
        public const string PatternFileName = @"[a-zA-Z0-9_ ]";
        
        /// <summary>
        /// Validate string as field name: can contain [a-z][A-Z][0-9] and underscore. Must not start with digit.
        /// </summary>
        public static bool IsValidFieldName(this string input) {
            return !string.IsNullOrEmpty(input) && input.HasRegexPattern(PatternFieldName);
        }

        public static bool HasRegexPattern(this string input, string pattern) {
            return input != null && Regex.Match(input, pattern).Success;
        }

        public static string ToStringNullSafe(this Object obj) {
            return obj == null ? "<null>" : obj.ToString();
        }
    }

}
