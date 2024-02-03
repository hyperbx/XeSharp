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
            Console = in_console;
            FriendlyName = GetFriendlyName();
        }

        // FIXME: this isn't working for subdirectories of drives.
        public string GetFriendlyName()
        {
            var data = XeFileSystem.Download(Console, $@"{Name}\name.txt");

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
