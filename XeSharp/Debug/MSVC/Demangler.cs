using System.Runtime.InteropServices;
using System.Text;

namespace XeSharp.Debug.MSVC
{
    public partial class Demangler
    {
        [LibraryImport("imagehlp.dll")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        private static partial uint UnDecorateSymbolName([MarshalAs(UnmanagedType.LPStr)] string in_name, [Out] byte[] out_undecoratedString, uint in_capacity, uint in_flags);

        public static string GetUndecoratedName(string in_mangledName, IEnumerable<EDemanglerFlags> in_flags)
        {
            var result = string.Empty;
            var flags = 0U;
            var demangledBytes = new byte[1024];

            foreach (var flag in in_flags)
                flags |= (uint)flag;

            // Remove '.' prefix to allow undecorator to work.
            in_mangledName = in_mangledName.TrimStart('.');

            // Fix undecorating template classes.
            if (in_mangledName.StartsWith("?AV?"))
                in_mangledName = "??" + in_mangledName[4..];

            while (true)
            {
                var resultLength = UnDecorateSymbolName
                (
                    in_mangledName,
                    demangledBytes,
                    (uint)demangledBytes.Length,
                    (uint)flags
                );

                if (resultLength == (demangledBytes.Length - 2))
                {
                    demangledBytes = new byte[demangledBytes.Length * 2];
                    continue;
                }
                else
                {
                    int count = Array.IndexOf<byte>(demangledBytes, 0, 0);

                    if (count < 0)
                        count = demangledBytes.Length;

                    result = Encoding.ASCII.GetString(demangledBytes, 0, count);

                    break;
                }
            }

            if (string.IsNullOrEmpty(result))
                return result;

            var namespaces = result.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

            if (namespaces.Length > 0)
            {
                var last = namespaces[^1];

                if (last.StartsWith("AV"))
                    namespaces[^1] = last.Remove(0, 2);

                result = string.Join("::", namespaces);
            }

            return result;
        }
    }
}
