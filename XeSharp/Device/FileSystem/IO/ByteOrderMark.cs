using System.Text;
using XeSharp.Helpers;

namespace XeSharp.Device.FileSystem.IO
{
    public class ByteOrderMark
    {
        public static Encoding GetEncoding(byte[] in_data)
        {
            switch (in_data[0])
            {
                case 0xEF: return Encoding.UTF8;
                case 0xFE: return Encoding.BigEndianUnicode;

                case 0xFF:
                {
                    if (in_data.Length < 2)
                        break;

                    if (in_data[1] != 0xFE)
                        break;

                    if (in_data.Length == 2)
                        return Encoding.Unicode;

                    return Encoding.UTF32;
                }

                case 0x00:
                {
                    if (in_data.Length != 4)
                        break;

                    if (in_data[3] != 0xFF)
                        break;

                    throw new NotSupportedException("UTF-32 (big-endian) encoding is not supported.");
                }

                case 0x2B: return Encoding.UTF7;
                case 0xF7: throw new NotSupportedException("UTF-1 encoding is not supported.");
                case 0xDD: throw new NotSupportedException("UTF-EBCDIC encoding is not supported.");
            }

            return Encoding.UTF8;
        }

        public static Encoding GetEncoding(uint in_signature)
        {
            return GetEncoding(MemoryHelper.UnmanagedTypeToByteArray(in_signature));
        }

        public static int GetSize(Encoding in_encoding)
        {
            if (in_encoding == Encoding.Unicode ||
                in_encoding == Encoding.BigEndianUnicode)
            {
                return 2;
            }
            else if (in_encoding == Encoding.UTF32)
            {
                return 4;
            }

            return 3;
        }

        public static string DecodeFromBOM(byte[] in_data)
        {
            var encoding = GetEncoding(in_data);

            return encoding.GetString(in_data.Skip(GetSize(encoding)).ToArray());
        }
    }
}
