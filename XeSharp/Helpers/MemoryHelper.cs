using System.Numerics;
using System.Runtime.InteropServices;

namespace XeSharp.Helpers
{
    public static class MemoryHelper
    {
        /// <summary>
        /// Prints a byte array to the console.
        /// </summary>
        /// <param name="in_data">The byte array to print.</param>
        /// <param name="in_baseAddr">The address to start the left-most column at.</param>
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

        /// <summary>
        /// Transforms a byte array to the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The type to transform the byte array to.</typeparam>
        /// <param name="in_data">The data to transform.</param>
        /// <param name="in_isBigEndian">Determines whether the data is in big-endian format.</param>
        public static T ByteArrayToUnmanagedType<T>(byte[] in_data, bool in_isBigEndian = false) where T : unmanaged
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

        /// <summary>
        /// Transforms an unmanaged object to a byte array.
        /// </summary>
        /// <typeparam name="T">The type to transform.</typeparam>
        /// <param name="in_structure">The data to transform.</param>
        /// <param name="in_isBigEndian">Determines whether the data is in big-endian format.</param>
        public static byte[] UnmanagedTypeToByteArray<T>(T in_structure, bool in_isBigEndian = true) where T : unmanaged
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

        /// <summary>
        /// Transforms a byte array into a string of hexadecimal bytes.
        /// </summary>
        /// <param name="in_data">The byte array to transform.</param>
        public static string ByteArrayToHexString(byte[] in_data, bool in_isSpaced = true)
        {
            return BitConverter.ToString(in_data).Replace("-", in_isSpaced ? " " : "");
        }

        /// <summary>
        /// Transforms a string of hexadecimal bytes into a byte array.
        /// </summary>
        /// <param name="in_hexStr">The string to transform.</param>
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

        /// <summary>
        /// Transforms a string of format "0x0000803F,0x00000040,0x00004040,0x00008040" into a Vector4.
        /// </summary>
        /// <param name="in_hexStr">The string to transform.</param>
        /// <param name="in_isSwapEndianness">Determines whether the endianness should be swapped for the final output.</param>
        public static Vector4 HexStringToVector4(string in_hexStr, bool in_isSwapEndianness = false)
        {
            var result = new Vector4();
            var split = in_hexStr.Split(',');

            if (split.Length <= 0 || split.Length < 4)
                return result;

            float x = ChangeType<float>(split[0]);
            float y = ChangeType<float>(split[1]);
            float z = ChangeType<float>(split[2]);
            float w = ChangeType<float>(split[3]);

            if (in_isSwapEndianness)
            {
                x = SwapEndianness(x);
                y = SwapEndianness(y);
                z = SwapEndianness(z);
                w = SwapEndianness(w);
            }

            result.X = x;
            result.Y = y;
            result.Z = z;
            result.W = w;

            return result;
        }

        /// <summary>
        /// Transforms a Vector4 into a string of format "0x0000803F,0x00000040,0x00004040,0x00008040".
        /// </summary>
        /// <param name="in_vector">The vector to transform.</param>
        /// <param name="in_isSwapEndianness">Determines whether the endianness should be swapped for the final output.</param>
        public static string Vector4ToHexString(Vector4 in_vector, bool in_isSwapEndianness = false)
        {
            var x = BitConverter.SingleToUInt32Bits(in_vector.X);
            var y = BitConverter.SingleToUInt32Bits(in_vector.Y);
            var z = BitConverter.SingleToUInt32Bits(in_vector.Z);
            var w = BitConverter.SingleToUInt32Bits(in_vector.W);

            if (in_isSwapEndianness)
            {
                x = SwapEndianness(x);
                y = SwapEndianness(y);
                z = SwapEndianness(z);
                w = SwapEndianness(w);
            }

            return $"0x{x:X8},0x{y:X8},0x{z:X8},0x{w:X8}";
        }

        /// <summary>
        /// Swaps the endianness of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the data to swap.</typeparam>
        /// <param name="in_data">The data to swap.</param>
        public static T SwapEndianness<T>(T in_data) where T : unmanaged
        {
            /* StructureToByteArray assumes big-endian by default,
               this should automatically reverse the given input. */
            return ByteArrayToUnmanagedType<T>(UnmanagedTypeToByteArray(in_data));
        }

        /// <summary>
        /// Transforms a string to the requested type.
        /// </summary>
        /// <param name="in_str">The string containing data to transform.</param>
        /// <param name="in_type">The type to transform to.</param>
        public static object ChangeType(string in_str, Type in_type)
        {
            if (in_str.StartsWith("0q"))
                in_str = "0x" + in_str.Remove(0, 2);

            var @base = in_str.StartsWith("0x") || in_str.EndsWith('h') ? 16 : 10;

            if (!in_type.Equals(typeof(string)) && in_str.EndsWith('h'))
                in_str = in_str[..^1];

            if (in_type.Equals(typeof(sbyte)))
                return Convert.ToSByte(in_str, @base);
            else if (in_type.Equals(typeof(byte)))
                return Convert.ToByte(in_str, @base);
            else if (in_type.Equals(typeof(byte[])))
                return HexStringToByteArray(in_str.StartsWith("0x") ? in_str.Remove(0, 2) : in_str);
            else if (in_type.Equals(typeof(bool)))
            {
                if (int.TryParse(in_str, out int out_boolean))
                    return out_boolean == 1;
            }
            else if (in_type.Equals(typeof(short)))
                return Convert.ToInt16(in_str, @base);
            else if (in_type.Equals(typeof(ushort)))
                return Convert.ToUInt16(in_str, @base);
            else if (in_type.Equals(typeof(int)))
                return Convert.ToInt32(in_str, @base);
            else if (in_type.Equals(typeof(uint)))
                return Convert.ToUInt32(in_str, @base);
            else if (in_type.Equals(typeof(float)) && @base == 16)
                return BitConverter.UInt32BitsToSingle(Convert.ToUInt32(in_str, @base));
            else if (in_type.Equals(typeof(long)))
                return Convert.ToInt64(in_str, @base);
            else if (in_type.Equals(typeof(ulong)))
                return Convert.ToUInt64(in_str, @base);
            else if (in_type.Equals(typeof(double)) && @base == 16)
                return BitConverter.UInt64BitsToDouble(Convert.ToUInt64(in_str, @base));
            else if (in_type.Equals(typeof(DateTime)))
                return FormatHelper.FromUnixTime(Convert.ToUInt64(in_str, @base));

            return Convert.ChangeType(in_str, in_type);
        }

        /// <summary>
        /// Transforms a string to the requested type.
        /// </summary>
        /// <typeparam name="T">The type to transform to.</typeparam>
        /// <param name="in_str">The string containing data to transform.</param>
        public static T ChangeType<T>(string in_str)
        {
            return (T)ChangeType(in_str, typeof(T));
        }
    }
}
