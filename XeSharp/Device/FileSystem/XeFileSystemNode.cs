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

        public bool IsRoot { get; internal set; }
        public XeDbgConsole Console { get; internal set; }
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
            IsRoot = in_node.IsRoot;
            Console = in_node.Console;
            Drive = in_node.Drive;
            Parent = in_node.Parent;
            Nodes = in_node.Nodes;
        }

        public XeFileSystemNode(XeDbgConsole in_console, string in_nodeCsv)
        {
            var ini = IniParser.DoInline(in_nodeCsv);

            Name = ini[""]["name"];

            if (string.IsNullOrEmpty(Name))
                throw new InvalidDataException("Node has no name.");

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
                Attributes |= EXeFileSystemNodeAttribute.ReadOnly;

            if (ini[""].ContainsKey("hidden"))
                Attributes |= EXeFileSystemNodeAttribute.Hidden;

            Console = in_console;
        }

        public XeDbgResponse Delete()
        {
            if (Type == EXeFileSystemNodeType.Directory)
            {
                Refresh();

                foreach (var node in Nodes)
                    node.Delete();
            }

            return XeFileSystem.Delete(Console, ToString(), Type);
        }

        public XeFileSystemNode Download()
        {
            Data = XeFileSystem.Download(Console, ToString());
            return this;
        }

        public void Download(Stream in_stream)
        {
            XeFileSystem.Download(Console, ToString(), in_stream);
        }

        public void Download(string in_destination, bool in_isOverwrite = true)
        {
            if (!FormatHelper.IsAbsolutePath(in_destination))
                in_destination = Path.Combine(Win32Helper.GetUserDirectory(), "Downloads", in_destination);

            if (!in_isOverwrite && File.Exists(in_destination))
                throw new IOException("The destination file already exists.");

            var targetDir = in_destination;

            void DownloadRecursive(XeFileSystemNode in_node, int in_relativeDepth = 1)
            {
                switch (in_node.Type)
                {
                    case EXeFileSystemNodeType.File:
                    {
                        var destination = Path.Combine(targetDir, in_node.GetRelativePath(in_relativeDepth));

                        using (var fileStream = File.OpenWrite(destination))
                            in_node.Download(fileStream);

                        in_node.SetLocalAttributes(destination);

                        break;
                    }

                    case EXeFileSystemNodeType.Directory:
                    {
                        in_node.Refresh();

                        var destination = Path.Combine(targetDir, in_node.GetRelativePath(in_relativeDepth));
                        Directory.CreateDirectory(destination);
                        in_node.SetLocalAttributes(destination);

                        foreach (var node in in_node.Nodes)
                            DownloadRecursive(node, in_relativeDepth + 1);

                        break;
                    }
                }
            }

            DownloadRecursive(this);
        }

        public XeFileSystemNode Refresh()
        {
            if (Type != EXeFileSystemNodeType.Directory)
                throw new NotSupportedException("The node must be a directory.");

            Nodes = IsRoot
                ? Console.FileSystem.GetDrivesRoot(Console.FileSystem.IsFlashMemoryMapped, false).Nodes
                : Console.FileSystem.GetNodesFromPath(ToString(), false, this);

            return this;
        }

        public void SetLocalAttributes(string in_path)
        {
            switch (Type)
            {
                case EXeFileSystemNodeType.File:
                {
                    File.SetCreationTime(in_path, DateCreated);
                    File.SetLastWriteTime(in_path, DateModified);

                    if (Attributes.HasFlag(EXeFileSystemNodeAttribute.ReadOnly))
                        File.SetAttributes(in_path, FileAttributes.ReadOnly);

                    if (Attributes.HasFlag(EXeFileSystemNodeAttribute.Hidden))
                        File.SetAttributes(in_path, FileAttributes.Hidden);

                    break;
                }

                case EXeFileSystemNodeType.Directory:
                {
                    Directory.SetCreationTime(in_path, DateCreated);
                    Directory.SetLastWriteTime(in_path, DateModified);

                    var info = new DirectoryInfo(in_path);

                    if (!info.Exists)
                        break;

                    if (Attributes.HasFlag(EXeFileSystemNodeAttribute.ReadOnly))
                        info.Attributes |= FileAttributes.ReadOnly;

                    if (Attributes.HasFlag(EXeFileSystemNodeAttribute.Hidden))
                        info.Attributes |= FileAttributes.Hidden;

                    break;
                }
            }
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

        public long GetTotalNodes(bool in_isRecursiveNodes = false, bool in_isFoldersIncluded = true)
        {
            var result = 0L;

            if (Type != EXeFileSystemNodeType.Directory)
                return 0;

            result += in_isFoldersIncluded
                ? Nodes.Count()
                : Nodes.Count(x => x.Type == EXeFileSystemNodeType.File);

            foreach (var node in Nodes)
            {
                if (node.Type == EXeFileSystemNodeType.File)
                    continue;

                if (in_isRecursiveNodes)
                    node.Refresh();

                result += node.GetTotalNodes(in_isRecursiveNodes, in_isFoldersIncluded);
            }

            return result;
        }

        public long GetTotalDataSize(bool in_isRecursiveNodes = false)
        {
            var result = 0L;

            result += Size;

            foreach (var node in Nodes)
            {
                if (node.Type == EXeFileSystemNodeType.Directory)
                {
                    if (in_isRecursiveNodes)
                        node.Refresh();

                    result += node.GetTotalDataSize(in_isRecursiveNodes);

                    continue;
                }

                result += node.Size;
            }

            return result;
        }

        public string GetRelativePath(int in_depth = -1)
        {
            var result = new List<string>();
            var node = this;

            while (node.Parent != null)
            {
                result.Add(node.Name);

                node = node.Parent;
                in_depth--;

                if (node is XeFileSystemDrive || in_depth == 0)
                    break;
            }

            result.Reverse();

            return string.Join('\\', result);
        }

        public string GetInfo()
        {
            var dataSize = GetTotalDataSize();

            var info = $"Name ---------- : {Name ?? "Unknown"}\n" +
                       $"Size ---------- : {FormatHelper.ByteLengthToDecimalString(dataSize)} ({dataSize:N0} bytes)\n" +
                       $"Date Created -- : {DateCreated:dd/MM/yyyy hh:mm tt}\n" +
                       $"Date Modified - : {DateModified:dd/MM/yyyy hh:mm tt}\n" +
                       $"Type ---------- : {Type}\n" +
                       $"Attributes ---- : {Attributes}";

            if (Type == EXeFileSystemNodeType.Directory)
                info += $"\nNodes --------- : {GetTotalNodes()}";

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
