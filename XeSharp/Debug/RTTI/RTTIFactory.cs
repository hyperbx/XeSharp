using XeSharp.Debug.MSVC;
using XeSharp.Debug.RTTI.Types;
using XeSharp.Device;

namespace XeSharp.Debug.RTTI
{
    public class RTTIFactory
    {
        /// <summary>
        /// Gets RTTI from the input vftable pointer.
        /// </summary>
        /// <param name="in_console">The console to read memory from.</param>
        /// <param name="in_pVftable">The pointer to the vftable with RTTI.</param>
        public static CompleteObjectLocator GetRuntimeInfoFromVftable(XeConsole in_console, uint in_pVftable)
        {
            if (in_pVftable == 0)
                return null;

            var addr = in_console.Read<uint>(in_pVftable - 0x04);

            if (!in_console.IsMemoryAccessible(addr))
                return null;

            return new CompleteObjectLocator(in_console, addr);
        }

        /// <summary>
        /// Gets RTTI from the input class pointer.
        /// </summary>
        /// <param name="in_console">The console to read memory from.</param>
        /// <param name="in_pClass">The pointer to the class where the first member is a pointer back to the vftable which has RTTI.</param>
        public static CompleteObjectLocator GetRuntimeInfoFromClass(XeConsole in_console, uint in_pClass)
        {
            return GetRuntimeInfoFromVftable(in_console, in_console.Read<uint>(in_pClass));
        }

        /// <summary>
        /// Gets the name of a class from its pointer.
        /// </summary>
        /// <param name="in_console">The console to read memory from.</param>
        /// <param name="in_pClass">The pointer to the class where the first member is a pointer back to the vftable which has RTTI.</param>
        /// <param name="in_isDemangled">Determines whether to demangle the MSVC name of the class.</param>
        /// <param name="in_demanglerFlags">The flags for the demangler.</param>
        public static string GetClassName(XeConsole in_console, uint in_pClass, bool in_isDemangled = true, IEnumerable<EDemanglerFlags> in_demanglerFlags = null)
        {
            var pRuntimeInfo = GetRuntimeInfoFromClass(in_console, in_pClass);

            if (pRuntimeInfo == null)
                return string.Empty;

            var pTypeDescriptor = pRuntimeInfo.GetTypeDescriptor();

            if (pTypeDescriptor == null)
                return string.Empty;

            return pTypeDescriptor.GetName(in_isDemangled, in_demanglerFlags);
        }

        /// <summary>
        /// Gets the namespaces of a class from its pointer.
        /// </summary>
        /// <param name="in_console">The console to read memory from.</param>
        /// <param name="in_pClass">The pointer to the class where the first member is a pointer back to the vftable which has RTTI.</param>
        public static string[] GetClassNamespaces(XeConsole in_console, uint in_pClass)
        {
            return GetClassName(in_console, in_pClass).Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
