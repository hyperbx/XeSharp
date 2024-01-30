using System.Diagnostics.CodeAnalysis;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.Title
{
    public class XeModuleInfo
    {
        [IniProperty(Key = "name")] public string Name { get; set; }
        [IniProperty(Key = "base")] public uint BaseAddress { get; set; }
        [IniProperty(Key = "size")] public uint ImageSize { get; set; }
        [IniProperty(Key = "check")] public uint Checksum { get; set; }
        [IniProperty(Key = "timestamp")] public DateTime Timestamp { get; set; }
        [IniProperty(Key = "pdata")] public uint PData { get; set; }
        [IniProperty(Key = "psize")] public uint PDataSize { get; set; }
        [IniProperty(Key = "thread", Alias = "dllthread")] public uint Thread { get; set; }
        [IniProperty(Key = "osize")] public uint OriginalSize { get; set; }
        public bool IsDLL { get; set; }

        public XeModuleInfo() { }

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
