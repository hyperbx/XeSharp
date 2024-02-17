using XeSharp.Device;

namespace XeSharp.Debug.RTTI.Types
{
    public class ClassHierarchyDescriptor
    {
        protected XeConsole _console;
        protected uint _pThis;

        public uint Signature;
        public uint Attributes;
        public uint BaseClassCount;
        public uint pBaseClasses;

        public ClassHierarchyDescriptor(XeConsole in_console, uint in_pThis)
        {
            _console = in_console;
            _pThis = in_pThis;

            Read();
        }

        private void Read()
        {
            Signature      = _console.Memory.Read<uint>(_pThis);
            Attributes     = _console.Memory.Read<uint>(_pThis + 0x04);
            BaseClassCount = _console.Memory.Read<uint>(_pThis + 0x08);
            pBaseClasses   = _console.Memory.Read<uint>(_pThis + 0x0C);
        }

        public BaseClassDescriptor GetBaseClass(int in_index)
        {
            if (in_index > BaseClassCount)
                return null;

            return new BaseClassDescriptor(_console, _console.Memory.Read<uint>(pBaseClasses + (uint)in_index * 4));
        }

        public BaseClassDescriptor[] GetBaseClasses()
        {
            var result = new List<BaseClassDescriptor>();

            for (int i = 0; i < BaseClassCount; i++)
                result.Add(GetBaseClass(i));

            return [.. result];
        }

        public override string ToString()
        {
            return $"{GetBaseClass(0).GetTypeDescriptor().GetName()}::`RTTI Class Hierarchy Descriptor'";
        }
    }
}
