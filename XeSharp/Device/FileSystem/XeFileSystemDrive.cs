using System.Text;

namespace XeSharp.Device.FileSystem
{
    public class XeFileSystemDrive : XeFileSystemNode
    {
        public string FriendlyName { get; internal set; }

        public override EXeFileSystemNodeType Type => EXeFileSystemNodeType.Directory;

        public XeFileSystemDrive() { }

        public XeFileSystemDrive(XeDbgConsole in_console, string in_name, List<XeFileSystemNode> in_nodes = null)
        {
            Name = in_name;
            Nodes = in_nodes;
            FriendlyName = GetFriendlyName(in_console);
        }

        public string GetFriendlyName(XeDbgConsole in_console)
        {
            var data = XeFileSystem.Download(in_console, $@"{Name}\name.txt");

            if (data == null || data.Length <= 0)
                return string.Empty;

            // Skip byte order mark.
            if (data[0] == 0xFE && data[1] == 0xFF)
                data = data.Skip(2).ToArray();

            return Encoding.BigEndianUnicode.GetString(data);
        }

        public override string ToString()
        {
            return base.ToString() + '\\';
        }
    }
}
