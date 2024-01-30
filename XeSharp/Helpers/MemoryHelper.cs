using System.Runtime.InteropServices;

namespace XeSharp.Helpers
{
    public static class MemoryHelper
    {
        public static void PrintBytes(byte[] in_data, uint in_baseAddr = 0)
        {
            var oldColour = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Address  ");

            // Print top row.
            for (int i = 0; i < 16; i++)
                Console.Write($"{(i + in_baseAddr % 16):X2} ");

            // Print top row for ASCII table.
            for (int i = 0; i < 16; i++)
                Console.Write($"{(((i + in_baseAddr) % 16 + 16) % 16):X}");

            Console.WriteLine();
            Console.ForegroundColor = oldColour;

            for (int i = 0; i < in_data.Length; i += 16)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{(in_baseAddr + i):X8} ");
                Console.ForegroundColor = oldColour;

                // Print bytes.
                for (int j = 0; j < 16; j++)
                {
                    int index = i + j;

                    if (index < in_data.Length)
                    {
                        Console.Write($"{in_data[index]:X2} ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("?? ");
                        Console.ForegroundColor = oldColour;
                    }
                }

                // Print ASCII table.
                for (int j = 0; j < 16; j++)
                {
                    int index = i + j;

                    if (index < in_data.Length)
                    {
                        char c = (char)in_data[index];
                        Console.Write(char.IsControl(c) ? '.' : c);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("?");
                        Console.ForegroundColor = oldColour;
                    }
                }

                Console.WriteLine();
            }
        }

        public static T ByteArrayToStructure<T>(byte[] in_data, bool in_isBigEndian = false) where T : struct
        {
            if (in_data == null || in_data.Length <= 0)
                return default;

            if (in_isBigEndian)
                in_data = in_data.Reverse().ToArray();

            var handle = GCHandle.Alloc(in_data, GCHandleType.Pinned);

            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] StructureToByteArray<T>(T in_structure, bool in_isBigEndian = true) where T : struct
        {
            byte[] data = new byte[Marshal.SizeOf(typeof(T))];

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(in_structure, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            return in_isBigEndian ? data.Reverse().ToArray() : data;
        }

        public static string ByteArrayToHexString(byte[] in_bytes)
        {
            return BitConverter.ToString(in_bytes).Replace("-", " ");
        }

        public static byte[] HexStringToByteArray(string in_hexStr)
        {
            in_hexStr = in_hexStr.Replace("0x", "")
                           .Replace(" ", "")
                           .Replace("?", "");

            return Enumerable.Range(0, in_hexStr.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(in_hexStr.Substring(x, 2), 16))
                             .ToArray();
        }

        public static object ChangeType(string in_str, Type in_type)
        {
            var @base = in_str.StartsWith("0x") || in_str.EndsWith('h') ? 16 : 10;

            if (in_str.EndsWith('h'))
                in_str = in_str[..^1];

            if (in_type.Equals(typeof(sbyte)))
                return Convert.ToSByte(in_str, @base);
            else if (in_type.Equals(typeof(byte)))
                return Convert.ToByte(in_str, @base);
            else if (in_type.Equals(typeof(short)))
                return Convert.ToInt16(in_str, @base);
            else if (in_type.Equals(typeof(ushort)))
                return Convert.ToUInt16(in_str, @base);
            else if (in_type.Equals(typeof(int)))
                return Convert.ToInt32(in_str, @base);
            else if (in_type.Equals(typeof(uint)))
                return Convert.ToUInt32(in_str, @base);
            else if (in_type.Equals(typeof(long)))
                return Convert.ToInt64(in_str, @base);
            else if (in_type.Equals(typeof(ulong)))
                return Convert.ToUInt64(in_str, @base);
            else if (in_type.Equals(typeof(DateTime)))
                return FormatHelper.FromUnixTime(Convert.ToUInt64(in_str, @base));

            return Convert.ChangeType(in_str, in_type);
        }

        public static T ChangeType<T>(string in_str)
        {
            return (T)ChangeType(in_str, typeof(T));
        }
    }
}
