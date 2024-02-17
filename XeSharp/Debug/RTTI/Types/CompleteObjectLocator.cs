using System.Text;
using XeSharp.Device;

namespace XeSharp.Debug.RTTI.Types
{
    public class CompleteObjectLocator
    {
        protected XeConsole _console;
        protected uint _pThis;

        public int Signature;
        public int VftableOffset;
        public int CtorDisplacementOffset;
        public uint pTypeDescriptor;
        public uint pClassHierarchyDescriptor;
        public uint pObjectBase;

        public CompleteObjectLocator(XeConsole in_console, uint in_pThis)
        {
            _console = in_console;
            _pThis = in_pThis;

            Read();
        }

        public void Read()
        {
            Signature                 = _console.Memory.Read<int>(_pThis);
            VftableOffset             = _console.Memory.Read<int>(_pThis + 0x04);
            CtorDisplacementOffset    = _console.Memory.Read<int>(_pThis + 0x08);
            pTypeDescriptor           = _console.Memory.Read<uint>(_pThis + 0x0C);
            pClassHierarchyDescriptor = _console.Memory.Read<uint>(_pThis + 0x10);
            pObjectBase               = _console.Memory.Read<uint>(_pThis + 0x14);
        }

        public TypeDescriptor GetTypeDescriptor()
        {
            return new TypeDescriptor(_console, pTypeDescriptor);
        }

        public ClassHierarchyDescriptor GetClassHierarchyDescriptor()
        {
            return new ClassHierarchyDescriptor(_console, pClassHierarchyDescriptor);
        }

        public string GetClassInfo()
        {
            var result = new StringBuilder();

            var typeDesc = GetTypeDescriptor();
            var hierarchy = GetClassHierarchyDescriptor();

            result.AppendLine(ToString());
            result.AppendLine($"  {typeDesc}");
            result.AppendLine($"    {hierarchy}");
            result.AppendLine($"      {typeDesc.GetName()}::`RTTI Base Class Array'");

            foreach (var @base in hierarchy.GetBaseClasses())
                result.AppendLine($"        {@base}");

            return result.ToString();
        }

        public override string ToString()
        {
            return $"const {GetTypeDescriptor().GetName()}::`RTTI Complete Object Locator'";
        }
    }
}
