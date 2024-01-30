namespace XeSharp.Helpers
{
    public class FormatHelper
    {
        public static DateTime FromUnixTime(ulong in_seconds, bool in_isLocalised = true)
        {
            var result = new DateTime(1970, 1, 1).AddSeconds(in_seconds);

            return in_isLocalised ? result.ToLocalTime() : result;
        }

        public static DateTime FromUnixTime(uint in_seconds, bool in_isLocalised = true)
        {
            return FromUnixTime((ulong)in_seconds, in_isLocalised);
        }

        public static DateTime FromFileTime(long in_intervals, bool in_isLocalised = true)
        {
            return in_isLocalised
                ? DateTime.FromFileTime(in_intervals)
                : DateTime.FromFileTimeUtc(in_intervals);
        }

        public static DateTime FromFileTime(uint in_hi, uint in_lo, bool in_isLocalised = true)
        {
            return FromFileTime(((long)in_hi << 32) | in_lo, in_isLocalised);
        }

        public static string IDAPatternToCodeMask(string in_pattern)
        {
            var mask = string.Empty;

            for (int i = 0; i < in_pattern.Length; i++)
            {
                if (in_pattern[i] == '?')
                {
                    mask += '?';
                    continue;
                }

                if (in_pattern[i] == ' ')
                    continue;

                mask += 'x';
                i++;
            }

            return mask;
        }

        public static string ByteLengthToDecimalString(long in_length)
        {
            string[] suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
            double readable = in_length;

            int i = 0;
            while (readable >= 1024 && i < suffixes.Length - 1)
            {
                readable /= 1024;
                i++;
            }

            return $"{readable:0} {suffixes[i]}";
        }

        public static bool IsAbsolutePath(string in_path)
        {
            return in_path.Split(['\\', '/'])[0].Contains(':');
        }

        public static bool IsAbsolutePath(string[] in_splitPath)
        {
            if (in_splitPath.Length <= 0)
                return false;

            return IsAbsolutePath(in_splitPath[0]);
        }

        public static bool IsAbsolutePath(List<string> in_splitPath)
        {
            return IsAbsolutePath(in_splitPath.ToArray());
        }
    }
}
