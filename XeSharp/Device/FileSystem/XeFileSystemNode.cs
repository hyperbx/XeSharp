using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.FileSystem
{
    public class XeFileSystemNode
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime DateCreated { get; set; } = new DateTime(1601, 1, 1);
        public DateTime DateModified { get; set; } = new DateTime(1601, 1, 1);
        public virtual EXeFileSystemNodeType Type { get; set; } = EXeFileSystemNodeType.File;
        public virtual EXeFileSystemNodeAttribute Attributes { get; set; } = EXeFileSystemNodeAttribute.None;

        public XeFileSystemDrive Drive { get; internal set; }
        public XeFileSystemNode Parent { get; internal set; }
        public IEnumerable<XeFileSystemNode> Nodes { get; set; } = [];

        public byte[] Data { get; internal set; }

        public XeFileSystemNode() { }

        public XeFileSystemNode(XeFileSystemNode in_node)
        {
            Name = in_node.Name;
            Size = in_node.Size;
            DateCreated = in_node.DateCreated;
            DateModified = in_node.DateModified;
            Type = in_node.Type;
            Attributes = in_node.Attributes;
            Drive = in_node.Drive;
            Parent = in_node.Parent;
            Nodes = in_node.Nodes;
        }

        public XeFileSystemNode(string in_nodeCsv)
        {
            var ini = IniParser.DoInline(in_nodeCsv);

            Name = ini[""]["name"];

            Size = ((long)MemoryHelper.ChangeType<uint>(ini[""]["sizehi"]) << 32) |
                MemoryHelper.ChangeType<uint>(ini[""]["sizelo"]);

            DateCreated = FormatHelper.FromFileTime(
                MemoryHelper.ChangeType<uint>(ini[""]["createhi"]),
                MemoryHelper.ChangeType<uint>(ini[""]["createlo"]));

            DateModified = FormatHelper.FromFileTime(
                MemoryHelper.ChangeType<uint>(ini[""]["changehi"]),
                MemoryHelper.ChangeType<uint>(ini[""]["changelo"]));

            if (ini[""].ContainsKey("directory"))
                Type = EXeFileSystemNodeType.Directory;

            if (ini[""].ContainsKey("readonly"))
                Attributes |= EXeFileSystemNodeAttribute.Readonly;

            if (ini[""].ContainsKey("hidden"))
                Attributes |= EXeFileSystemNodeAttribute.Hidden;
        }

        public XeDbgResponse Delete(XeDbgConsole in_console)
        {
            if (Type == EXeFileSystemNodeType.Directory)
            {
                Refresh(in_console);

                foreach (var node in Nodes)
                    node.Delete(in_console);
            }

            return XeFileSystem.Delete(in_console, ToString(), Type);
        }

        public XeFileSystemNode Download(XeDbgConsole in_console)
        {
            Data = XeFileSystem.Download(in_console, ToString());
            return this;
        }

        public XeFileSystemNode Refresh(XeDbgConsole in_console)
        {
            if (Type != EXeFileSystemNodeType.Directory)
                throw new NotSupportedException("The node must be a directory.");

            Nodes = in_console.FileSystem.GetNodesFromPath(ToString(), false, this);

            return this;
        }

        public XeFileSystemDrive GetDrive()
        {
            var node = this;

            while (node.Parent != null)
            {
                node = node.Parent;

                if (node is XeFileSystemDrive drive)
                    return drive;
            }

            return null;
        }

        public XeFileSystemNode GetRoot()
        {
            var node = this;

            while (node.Parent != null)
                node = node.Parent;

            return node;
        }

        public string GetInfo()
        {
            var info = $"Name ---------- : {Name ?? "Unknown"}\n" +
                       $"Size ---------- : {FormatHelper.ByteLengthToDecimalString(Size)} ({Size:N0} bytes)\n" +
                       $"Date Created -- : {DateCreated:dd/MM/yyyy hh:mm tt}\n" +
                       $"Date Modified - : {DateModified:dd/MM/yyyy hh:mm tt}\n" +
                       $"Type ---------- : {Type}\n" +
                       $"Attributes ---- : {Attributes}";

            if (Type == EXeFileSystemNodeType.Directory)
            {
                info += '\n';
                info += $"Nodes --------- : {Nodes.Count()}";
            }

            return info;
        }

        public override string ToString()
        {
            if (Name == null)
                return string.Empty;

            return Parent == null ? Name : Path.Combine(Parent.ToString(), Name);
        }
    }
}
