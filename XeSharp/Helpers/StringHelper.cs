using System.Text.RegularExpressions;

namespace XeSharp.Helpers
{
    public class StringHelper
    {
        public static string[] ParseArgs(string in_line, bool in_isTrimmedQuotes = true)
        {
            var pattern = @"(?:[^\s""]+|""(?:[^""]|"""")*"")";
            var matches = Regex.Matches(in_line, pattern);
            var args = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
                args[i] = in_isTrimmedQuotes ? matches[i].Value.Trim('\"') : matches[i].Value;

            return args;
        }
    }
}
