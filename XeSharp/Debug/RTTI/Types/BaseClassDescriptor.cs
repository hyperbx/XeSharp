using XeSharp.Device;

namespace XeSharp.Debug.RTTI.Types
{
    public class BaseClassDescriptor
    {
        protected XeConsole _console;
        protected uint _pThis;

        public uint pTypeDescriptor;
        public int SubElementCount;
        public int MemberDisplacement;
        public int VftableDisplacement;
        public int DisplacementWithinVftable;
        public int BaseClassAttributes;
        public uint pClassHierarchyDescriptor;

        public BaseClassDescriptor(XeConsole in_console, uint in_pThis)
        {
            _console = in_console;
            _pThis = in_pThis;

            Read();
        }

        private void Read()
        {
            pTypeDescriptor           = _console.Memory.Read<uint>(_pThis);
            SubElementCount           = _console.Memory.Read<int>(_pThis + 0x04);
            MemberDisplacement        = _console.Memory.Read<int>(_pThis + 0x08);
            VftableDisplacement       = _console.Memory.Read<int>(_pThis + 0x0C);
            DisplacementWithinVftable = _console.Memory.Read<int>(_pThis + 0x10);
            BaseClassAttributes       = _console.Memory.Read<int>(_pThis + 0x14);
            pClassHierarchyDescriptor = _console.Memory.Read<uint>(_pThis + 0x18);
        }

        public TypeDescriptor GetTypeDescriptor()
        {
            return new TypeDescriptor(_console, pTypeDescriptor);
        }

        public ClassHierarchyDescriptor GetClassHierarchyDescriptor()
        {
            return new ClassHierarchyDescriptor(_console, pClassHierarchyDescriptor);
        }

        public override string ToString()
        {
            return $"{GetTypeDescriptor().GetName()}::`RTTI Base Class Descriptor at ({MemberDisplacement}, {VftableDisplacement}, {DisplacementWithinVftable}, {BaseClassAttributes})'";
        }
    }
}
