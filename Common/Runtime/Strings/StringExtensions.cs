using System.Text.RegularExpressions;

namespace MisterGames.Common.Strings {

    public static class StringExtensions {

        public const string PatternFieldName = @"^([a-zA-Z_])([a-zA-Z0-9_]*)";
        public const string PatternFileName = @"[a-zA-Z0-9_ ]";

        public static bool HasRegexPattern(this string input, string pattern) {
            return input != null && Regex.Match(input, pattern).Success;
        }
        
        public static bool IsSubPathOf(this string sub, string parent, char separator) {
            int parentLength = parent.Length;
            if (parentLength == 0) return true;
            
            int subLength = sub.Length;
            if (parentLength > subLength) return false;
            if (parentLength == subLength) return sub == parent;

            return sub[parentLength] == separator && parent == sub[..parentLength];
        }
    }

}
