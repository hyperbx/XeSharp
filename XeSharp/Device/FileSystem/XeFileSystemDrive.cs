using System.Text;
using XeSharp.Helpers;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.FileSystem
{
    public class XeFileSystemDrive : XeFileSystemNode
    {
        /// <summary>
        /// The user-defined name of this drive.
        /// </summary>
        public string FriendlyName { get; internal set; }

        /// <summary>
        /// The total bytes free for this drive.
        /// </summary>
        public ulong FreeSpace { get; internal set; }

        /// <summary>
        /// The total bytes used for this drive.
        /// </summary>
        public ulong UsedSpace => Capacity - FreeSpace;

        /// <summary>
        /// The capacity of this drive.
        /// </summary>
        public ulong Capacity { get; internal set; }

        public override EXeFileSystemNodeType Type => EXeFileSystemNodeType.Directory;

        public XeFileSystemDrive() { }

        /// <summary>
        /// Creates a new drive instance.
        /// </summary>
        /// <param name="in_console">The console this drive belongs to.</param>
        /// <param name="in_name">The name of this drive.</param>
        /// <param name="in_nodes">The nodes in this drive.</param>
        public XeFileSystemDrive(XeDbgConsole in_console, string in_name, List<XeFileSystemNode> in_nodes = null)
        {
            Name = in_name;
            Nodes = in_nodes;
            Console = in_console;
            FriendlyName = GetFriendlyName();

            Parse((string)in_console.Client.SendCommand($"drivefreespace name=\"{ToString()}\"").Results[0]);
        }

        /// <summary>
        /// Parses drive capacity information.
        /// </summary>
        /// <param name="in_driveCsv">The space-separated values for information about this drive's capacity.</param>
        public void Parse(string in_driveCsv)
        {
            var ini = IniParser.DoInline(in_driveCsv);

            FreeSpace = ((ulong)MemoryHelper.ChangeType<uint>(ini[""]["totalfreebyteshi"]) << 32) |
                MemoryHelper.ChangeType<uint>(ini[""]["totalfreebyteslo"]);

            Capacity = ((ulong)MemoryHelper.ChangeType<uint>(ini[""]["totalbyteshi"]) << 32) |
                MemoryHelper.ChangeType<uint>(ini[""]["totalbyteslo"]);
        }

        /// <summary>
        /// Gets the user-defined name of this drive.
        /// </summary>
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
