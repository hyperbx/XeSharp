using ClassAnalyser.Analysis.RTTI;
using System.Text;
using XeSharp.Device;

namespace XeSharp.Debug.Analysis
{
    public class RTTIReaderXenon(XeConsole in_console) : IRTTIReader
    {
        protected XeConsole _console = in_console;

        public nuint GetBaseAddress()
        {
            return 0;
        }

        public nuint GetPointerSize()
        {
            return 4;
        }

        public bool IsMemoryAccessible(nuint in_address)
        {
            return _console.Memory.IsAccessible((uint)in_address);
        }

        public T Read<T>(nuint in_address) where T : unmanaged
        {
            return _console.Memory.Read<T>((uint)in_address);
        }

        public nuint ReadPointer(nuint in_address)
        {
            return _console.Memory.Read<uint>((uint)in_address);
        }

        public string ReadStringNullTerminated(nuint in_address, Encoding in_encoding = null)
        {
            return _console.Memory.ReadStringNullTerminated((uint)in_address, in_encoding);
        }
    }
}