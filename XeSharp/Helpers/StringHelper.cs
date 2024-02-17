using System.Text.RegularExpressions;

namespace XeSharp.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Parses arguments from a string, whilst preserving quotes as individual entries.
        /// </summary>
        /// <param name="in_line">The string to parse arguments from.</param>
        /// <param name="in_isTrimmedQuotes">Determines whether entries wrapped in quotes get trimmed.</param>
        public static string[] ParseArgs(string in_line, bool in_isTrimmedQuotes = true)
        {
            var pattern = @"(?:[^\s""]+|""(?:[^""]|"""")*"")";
            var matches = Regex.Matches(in_line, pattern);
            var args = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
                args[i] = in_isTrimmedQuotes ? matches[i].Value.Trim('\"') : matches[i].Value;

            return args;
        }

        /// <summary>
        /// Determines whether the input string is null, empty or is a whitespace character.
        /// </summary>
        /// <param name="in_str">The string to check.</param>
        public static bool IsNullOrEmptyOrWhiteSpace(this string in_str)
        {
            return string.IsNullOrEmpty(in_str) || string.IsNullOrWhiteSpace(in_str);
        }

        /// <summary>
        /// Gets the total amount of dereferences to a given pointer.
        /// </summary>
        /// <param name="in_str">A string containing an address or object name surrounded by square brackets to indicate a dereferenced pointer (e.g. [0x82000000], [[GPR3]]).</param>
        public static int GetDereferenceCount(string in_str)
        {
            var result = 0;

            while (in_str.StartsWith('[') && in_str.EndsWith(']'))
            {
                in_str = in_str[1..^1];
                result++;
            }

            return result;
        }

        /// <summary>
        /// Trims all dereference characters from a given string.
        /// </summary>
        /// <param name="in_str">The string to trim.</param>
        public static string TrimDereferences(string in_str)
        {
            return in_str.TrimStart('[').TrimEnd(']');
        }
    }
}
