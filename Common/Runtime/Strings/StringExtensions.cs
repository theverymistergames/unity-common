using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

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

        public static string ToBitString(this long value, int bits = 64) => GetBitString(value, Mathf.Min(bits, 64));
        public static string ToBitString(this int value, int bits = 64) => GetBitString(value, Mathf.Min(bits, 32));
        public static string ToBitString(this short value, int bits = 64) => GetBitString(value, Mathf.Min(bits, 16));
        public static string ToBitString(this byte value, int bits = 64) => GetBitString(value, Mathf.Min(bits, 8));
        
        private static string GetBitString(long value, int bits) {
            var sb = new StringBuilder();

            for (int i = bits - 1; i >= 0; i--) {
                sb.Append((value & (1 << i)) == 0 ? '0' : '1');
            }
            
            return sb.ToString();
        }
    }

}
