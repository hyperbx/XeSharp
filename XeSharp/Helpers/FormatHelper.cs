using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace XeSharp.Helpers
{
    public class FormatHelper
    {
        /// <summary>
        /// Transforms seconds to Unix time.
        /// </summary>
        /// <param name="in_seconds">The amount of seconds since 01/01/1970.</param>
        /// <param name="in_isLocalised">Determines whether the result will be localised to the current time zone.</param>
        public static DateTime FromUnixTime(ulong in_seconds, bool in_isLocalised = true)
        {
            var result = new DateTime(1970, 1, 1).AddSeconds(in_seconds);

            return in_isLocalised ? result.ToLocalTime() : result;
        }

        /// <summary>
        /// Transforms seconds to Unix time.
        /// </summary>
        /// <param name="in_seconds">The amount of seconds since 01/01/1970.</param>
        /// <param name="in_isLocalised">Determines whether the result will be localised to the current time zone.</param>
        public static DateTime FromUnixTime(uint in_seconds, bool in_isLocalised = true)
        {
            return FromUnixTime((ulong)in_seconds, in_isLocalised);
        }

        /// <summary>
        /// Transforms 100-nanosecond intervals to Win32 file time.
        /// </summary>
        /// <param name="in_intervals">The amount of 100-nanosecond intervals since 01/01/1601.</param>
        /// <param name="in_isLocalised">Determines whether the result will be localised to the current time zone.</param>
        public static DateTime FromFileTime(long in_intervals, bool in_isLocalised = true)
        {
            return in_isLocalised
                ? DateTime.FromFileTime(in_intervals)
                : DateTime.FromFileTimeUtc(in_intervals);
        }

        /// <summary>
        /// Transforms 100-nanosecond intervals to Win32 file time.
        /// </summary>
        /// <param name="in_intervals">The amount of 100-nanosecond intervals since 01/01/1601.</param>
        /// <param name="in_isLocalised">Determines whether the result will be localised to the current time zone.</param>
        public static DateTime FromFileTime(uint in_hi, uint in_lo, bool in_isLocalised = true)
        {
            return FromFileTime(((long)in_hi << 32) | in_lo, in_isLocalised);
        }

        /// <summary>
        /// Transforms an IDA pattern to a code pattern mask.
        /// </summary>
        /// <param name="in_pattern">The IDA pattern to transform.</param>
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

        /// <summary>
        /// Gets a readable unit from a length of bytes.
        /// </summary>
        /// <param name="in_length">The length of bytes.</param>
        public static (double Readable, string Unit) ByteLengthToDecimalUnits(ulong in_length)
        {
            string[] suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
            double readable = in_length;

            int i = 0;
            while (readable >= 1000 && i < suffixes.Length - 1)
            {
                readable /= 1000;
                i++;
            }

            return (readable, suffixes[i]);
        }

        /// <summary>
        /// Creates a readable string from a length of bytes.
        /// </summary>
        /// <param name="in_length">The length of bytes.</param>
        public static string ByteLengthToDecimalString(ulong in_length)
        {
            var (readable, unit) = ByteLengthToDecimalUnits(in_length);

            return $"{readable:0} {unit}";
        }

        /// <summary>
        /// Determines whether the input path is considered an absolute path.
        /// </summary>
        /// <param name="in_path">The path to check.</param>
        public static bool IsAbsolutePath(string in_path)
        {
            return in_path.Split(['\\', '/'])[0].Contains(':');
        }

        /// <summary>
        /// Determines whether the input path sequence is considered an absolute path.
        /// </summary>
        /// <param name="in_splitPath">The paths to check.</param>
        public static bool IsAbsolutePath(string[] in_splitPath)
        {
            if (in_splitPath.Length <= 0)
                return false;

            return IsAbsolutePath(in_splitPath[0]);
        }

        /// <summary>
        /// Determines whether the input path sequence is considered an absolute path.
        /// </summary>
        /// <param name="in_splitPath">The paths to check.</param>
        public static bool IsAbsolutePath(List<string> in_splitPath)
        {
            return IsAbsolutePath(in_splitPath.ToArray());
        }
    }
}
