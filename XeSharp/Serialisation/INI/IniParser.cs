using System.Text.RegularExpressions;
using XeSharp.Helpers;

namespace XeSharp.Serialisation.INI
{
    public class IniParser
    {
        /// <summary>
        /// Parses INI data into a dictionary.
        /// </summary>
        /// <param name="in_ini">The lines to parse.</param>
        public static Dictionary<string, Dictionary<string, string>> DoLines(string[] in_ini)
        {
            Dictionary<string, Dictionary<string, string>> result = [];

            string section = string.Empty;

            // Add root section.
            result.Add(section, []);

            int i = 0;
            foreach (var line in in_ini)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    section = line[1..^1];
                    result.Add(section, []);
                    continue;
                }

                if (line.StartsWith(';') || line.StartsWith('#'))
                    continue;

                string key;
                string value;
                int delimiterIndex = line.IndexOf('=');

                if (delimiterIndex == -1)
                {
                    key = line;
                    value = string.Empty;
                }
                else
                {
                    key = line[..delimiterIndex];
                    value = line[(delimiterIndex + 1)..];
                }

                if (value.Length > 1 && value.StartsWith('\"') && value.EndsWith('\"'))
                    value = value[1..^1];

                if (result[section].ContainsKey(key))
                    key += i;

                result[section].Add(key, value);

                i++;
            }

            return result;
        }

        /// <summary>
        /// Parses INI data into a class using <see cref="IniPropertyAttribute"/> on its members.
        /// </summary>
        /// <typeparam name="T">The class type.</typeparam>
        /// <param name="in_obj">The class using <see cref="IniPropertyAttribute"/> on its members.</param>
        /// <param name="in_ini">The lines to parse.</param>
        public static void DoLines<T>(T in_obj, string[] in_ini)
        {
            var ini = DoLines(in_ini);

            foreach (var property in typeof(T).GetProperties())
            {
                var iniAttribute = (IniPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(IniPropertyAttribute));

                if (iniAttribute == null)
                    continue;

                string section = iniAttribute.Section;
                string key = string.IsNullOrEmpty(iniAttribute.Key) ? property.Name : iniAttribute.Key;

                if (!ini.TryGetValue(section, out Dictionary<string, string>? out_keys))
                    continue;

                if (!out_keys.TryGetValue(key, out string? out_value) && !out_keys.TryGetValue(iniAttribute.Alias, out out_value))
                    continue;

                // Just in case.
                if (string.IsNullOrEmpty(out_value))
                    continue;

                property.SetValue(in_obj, MemoryHelper.ChangeType(out_value, property.PropertyType));
            }
        }

        private static string[] SplitInlineIni(string in_line)
        {
            var pattern = @"\s*([^=\s]+)\s*(=\s*(""[^""]*""|\S+))?\s*";
            var matches = Regex.Matches(in_line, pattern);
            var entries = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
                entries[i] = matches[i].Value.Trim();

            return entries;
        }

        /// <summary>
        /// Parses INI data written on a single line using spaces instead of line breaks.
        /// </summary>
        /// <param name="in_line">The line to parse.</param>
        public static Dictionary<string, Dictionary<string, string>> DoInline(string in_line)
        {
            return DoLines(SplitInlineIni(in_line));
        }

        /// <summary>
        /// Parses INI data written on a single line using spaces instead of line breaks.
        /// </summary>
        /// <typeparam name="T">The class type.</typeparam>
        /// <param name="in_obj">The class using <see cref="IniPropertyAttribute"/> on its members.</param>
        /// <param name="in_line">The line to parse.</param>
        public static void DoInline<T>(T in_obj, string in_line)
        {
            DoLines(in_obj, SplitInlineIni(in_line));
        }

        /// <summary>
        /// Parses INI data from a file.
        /// </summary>
        /// <param name="in_path">The path to the *.ini file to parse.</param>
        public static Dictionary<string, Dictionary<string, string>> DoFile(string in_path)
        {
            if (!File.Exists(in_path))
                return [];

            return DoLines(File.ReadAllLines(in_path));
        }

        /// <summary>
        /// Parses INI data from a file.
        /// </summary>
        /// <typeparam name="T">The class type.</typeparam>
        /// <param name="in_obj">The class using <see cref="IniPropertyAttribute"/> on its members.</param>
        /// <param name="in_path">The path to the *.ini file to parse.</param>
        public static void DoFile<T>(T in_obj, string in_path)
        {
            if (!File.Exists(in_path))
                return;

            DoLines(in_obj, File.ReadAllLines(in_path));
        }
    }
}
