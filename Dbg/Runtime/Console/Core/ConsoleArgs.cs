using System.Collections.Generic;
using System.Text;

namespace MisterGames.Dbg.Console.Core {

    public static class ConsoleArgs {

        public const char Separator = ' ';
        public const char Quote = '"';

        /// <summary>
        /// Splits text into arguments by spaces, text inside double quotes is a single argument.
        /// </summary>
        /// <param name="text">Text to parse.</param>
        /// <param name="startIndex">Index in text to start parsing from.</param>
        /// <param name="args">Parsed arguments without quotes.</param>
        /// <param name="argStarts">Optional indices in text where each argument starts, including opening quote.</param>
        /// <returns>True if text ends inside an unclosed quote.</returns>
        public static bool Parse(string text, int startIndex, List<string> args, List<int> argStarts = null) {
            var sb = new StringBuilder();
            bool inQuotes = false;
            bool hasArg = false;
            int argStart = 0;

            for (int i = startIndex; i < text.Length; i++) {
                char c = text[i];

                if (c == Separator && !inQuotes) {
                    if (!hasArg) continue;

                    args.Add(sb.ToString());
                    argStarts?.Add(argStart);

                    sb.Clear();
                    hasArg = false;
                    continue;
                }

                if (!hasArg) {
                    hasArg = true;
                    argStart = i;
                }

                if (c == Quote) {
                    inQuotes = !inQuotes;
                    continue;
                }

                sb.Append(c);
            }

            if (hasArg) {
                args.Add(sb.ToString());
                argStarts?.Add(argStart);
            }

            return inQuotes;
        }

        /// <summary>
        /// Wraps value into double quotes if it contains spaces, so it is parsed as a single argument.
        /// </summary>
        public static string QuoteIfNeeded(string value) {
            return value.IndexOf(Separator) >= 0 ? $"{Quote}{value}{Quote}" : value;
        }
    }

}
