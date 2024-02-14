using System.Diagnostics.CodeAnalysis;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.Title
{
    public class XeModuleInfo
    {
        /// <summary>
        /// The name of this module.
        /// </summary>
        [IniProperty(Key = "name")] public string Name { get; set; }

        /// <summary>
        /// The address this module starts in memory.
        /// </summary>
        [IniProperty(Key = "base")] public uint BaseAddress { get; set; }

        /// <summary>
        /// The size of this module in memory.
        /// </summary>
        [IniProperty(Key = "size")] public uint ImageSize { get; set; }

        /// <summary>
        /// The checksum pertaining to this module.
        /// </summary>
        [IniProperty(Key = "check")] public uint Checksum { get; set; }

        /// <summary>
        /// The date and time this module was compiled on.
        /// </summary>
        [IniProperty(Key = "timestamp")] public DateTime Timestamp { get; set; }

        /// <summary>
        /// TODO: figure out what this is.
        /// </summary>
        [IniProperty(Key = "pdata")] public uint PData { get; set; }

        /// <summary>
        /// TODO: figure out what this is.
        /// </summary>
        [IniProperty(Key = "psize")] public uint PDataSize { get; set; }

        /// <summary>
        /// The thread this module is running on.
        /// </summary>
        [IniProperty(Key = "thread", Alias = "dllthread")] public int Thread { get; set; }

        /// <summary>
        /// The original size of this module.
        /// </summary>
        [IniProperty(Key = "osize")] public uint OriginalSize { get; set; }

        /// <summary>
        /// Determines whether this module is a DLL.
        /// </summary>
        public bool IsDLL { get; set; }

        public XeModuleInfo() { }

        /// <summary>
        /// Creates a new module from space-separated values.
        /// </summary>
        /// <param name="in_moduleCsv">The space-separated values for information about this module.</param>
        public XeModuleInfo(string in_moduleCsv)
        {
            IniParser.DoInline(this, in_moduleCsv);

            IsDLL = in_moduleCsv.Contains("dllthread");
        }

        public override bool Equals([NotNullWhen(true)] object? in_obj)
        {
            if (in_obj is XeModuleInfo moduleInfo)
            {
                return Name == moduleInfo.Name &&
                       BaseAddress == moduleInfo.BaseAddress &&
                       ImageSize == moduleInfo.ImageSize &&
                       Checksum == moduleInfo.Checksum &&
                       Timestamp == moduleInfo.Timestamp &&
                       PData == moduleInfo.PData &&
                       PDataSize == moduleInfo.PDataSize &&
                       Thread == moduleInfo.Thread &&
                       OriginalSize == moduleInfo.OriginalSize &&
                       IsDLL == moduleInfo.IsDLL;
            }

            return false;
        }
    }
}
