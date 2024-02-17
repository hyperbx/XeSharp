using XeSharp.Debug.MSVC;
using XeSharp.Device;

namespace XeSharp.Debug.RTTI.Types
{
    public class TypeDescriptor
    {
        protected XeConsole _console;
        protected uint _pThis;

        public uint pTypeInfo;
        public uint pRuntimeRef;

        public TypeDescriptor(XeConsole in_console, uint in_pThis)
        {
            _console = in_console;
            _pThis = in_pThis;

            Read();
        }

        private void Read()
        {
            pTypeInfo   = _console.Memory.Read<uint>(_pThis);
            pRuntimeRef = _console.Memory.Read<uint>(_pThis + 0x04);
        }

        public string GetName(bool in_isDemangled = true, IEnumerable<EDemanglerFlags> in_demanglerFlags = null)
        {
            var result = _console.Memory.ReadStringNullTerminated(_pThis + 0x08);

            if (string.IsNullOrEmpty(result))
                return string.Empty;

            if (in_isDemangled)
            {
                return Demangler.GetUndecoratedName(result, in_demanglerFlags ?? new[] { EDemanglerFlags.NameOnly });
            }
            else
            {
                return result;
            }
        }

        public string[] GetNamespaces()
        {
            return GetName().Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public override string ToString()
        {
            return $"class {GetName()} `RTTI Type Descriptor'";
        }
    }
}
