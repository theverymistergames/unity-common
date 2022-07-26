using System.Text.RegularExpressions;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Common.Strings {

    public static class StringUtils {

        public const string PatternFieldName = @"^([a-zA-Z_])([a-zA-Z0-9_]*)";
        public const string PatternFileName = @"[a-zA-Z0-9_ ]";
        
        /// <summary>
        /// Validate string as field name: can contain [a-z][A-Z][0-9] and underscore. Must not start with digit.
        /// </summary>
        public static bool IsValidFieldName(this string input) {
            return input != null && input.IsValidForPattern(PatternFieldName);
        }

        public static bool IsValidForPattern(this string input, string pattern) {
            return input != null && Regex.Match(input, pattern).Success;
        }

        public static string ToStringNullSafe(this Object obj) {
            return obj == null ? "<null>" : obj.ToString();
        }

        public static string UpperFirstLetter(this string str) {
            if (str.IsEmpty()) return str;
            var firstLetter = char.ToUpper(str[0]).ToString();
            return str.HasOneElement() ? firstLetter : $"{firstLetter}{str.Substring(1)}";
        }
        
    }

}