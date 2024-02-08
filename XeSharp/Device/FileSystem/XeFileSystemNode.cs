using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.FileSystem
{
    public class XeFileSystemNode
    {
        /// <summary>
        /// The name of this node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The size (in bytes) of this node.
        /// </summary>
        public ulong Size { get; set; }

        /// <summary>
        /// The date and time this node was created.
        /// </summary>
        public DateTime DateCreated { get; set; } = new DateTime(1601, 1, 1);

        /// <summary>
        /// The date and time this node was last modified.
        /// </summary>
        public DateTime DateModified { get; set; } = new DateTime(1601, 1, 1);

        /// <summary>
        /// The type this node is.
        /// </summary>
        public virtual EXeFileSystemNodeType Type { get; set; } = EXeFileSystemNodeType.File;

        /// <summary>
        /// The attributes pertaining to this node.
        /// </summary>
        public virtual EXeFileSystemNodeAttribute Attributes { get; set; } = EXeFileSystemNodeAttribute.None;

        /// <summary>
        /// Determines whether this node is the root directory.
        /// <para>Used for the root node from <see cref="XeFileSystem.GetDrivesRoot(bool, bool)"/>.</para>
        /// </summary>
        public bool IsRoot { get; internal set; }

        /// <summary>
        /// The console pertaining to this node.
        /// </summary>
        public XeDbgConsole Console { get; internal set; }

        /// <summary>
        /// The drive pertaining to this node.
        /// </summary>
        public XeFileSystemDrive Drive { get; internal set; }

        /// <summary>
        /// The parent containing this node.
        /// </summary>
        public XeFileSystemNode Parent { get; internal set; }

        /// <summary>
        /// The subnodes pertaining to this node (if it is a directory).
        /// </summary>
        public IEnumerable<XeFileSystemNode> Nodes { get; set; } = [];

        /// <summary>
        /// The binary data pertaining to this node.
        /// </summary>
        public byte[] Data { get; internal set; }

        public XeFileSystemNode() { }

        /// <summary>
        /// Clones a node.
        /// </summary>
        /// <param name="in_node">The node to clone.</param>
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

        /// <summary>
        /// Creates a new node from space-separated values.
        /// </summary>
        /// <param name="in_console">The console pertaining to this node.</param>
        /// <param name="in_nodeCsv">The space-separated values for information about this node.</param>
        public XeFileSystemNode(XeDbgConsole in_console, string in_nodeCsv)
        {
            var ini = IniParser.DoInline(in_nodeCsv);

            Name = ini[""]["name"];

            if (string.IsNullOrEmpty(Name))
                throw new InvalidDataException("Node has no name.");

            Size = ((ulong)MemoryHelper.ChangeType<uint>(ini[""]["sizehi"]) << 32) |
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

        /// <summary>
        /// Deletes this node.
        /// </summary>
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

        /// <summary>
        /// Downloads the contents of this node into the <see cref="Data"/> buffer.
        /// <para>Not recommended for large files.</para>
        /// </summary>
        public XeFileSystemNode Download()
        {
            Data = XeFileSystem.Download(Console, ToString());
            return this;
        }

        /// <summary>
        /// Downloads the contents of this node into the input stream.
        /// <para>Recommended for large files.</para>
        /// </summary>
        /// <param name="in_stream">The stream to write this node's data to.</param>
        public void Download(Stream in_stream)
        {
            XeFileSystem.Download(Console, ToString(), in_stream);
        }

        /// <summary>
        /// Downloads the contents of this node to a local path.
        /// </summary>
        /// <param name="in_destination">The path to download to.</param>
        /// <param name="in_isOverwrite">Determines whether local files can be overwritten if they already exist.</param>
        public void Download(string in_destination, bool in_isOverwrite = true)
        {
            if (!FormatHelper.IsAbsolutePath(in_destination))
                in_destination = Path.Combine(Win32Helper.GetUserDirectory(), "Downloads");

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

            ExceptionHelper.OperationCancelledHandler(
                () => DownloadRecursive(this),
                () => Console.Client.Flush());
        }

        /// <summary>
        /// Refreshes the contents of this node (if it is a directory).
        /// </summary>
        public XeFileSystemNode Refresh()
        {
            if (Type != EXeFileSystemNodeType.Directory)
                return this;

            Nodes = IsRoot
                ? Console.FileSystem.GetDrivesRoot(Console.FileSystem.IsFlashMemoryMapped, false).Nodes
                : Console.FileSystem.GetNodesFromPath(ToString(), false, this);

            return this;
        }

        /// <summary>
        /// Sets the attributes of a local file or directory to match this node's attributes.
        /// </summary>
        /// <param name="in_path">The path to set attributes to.</param>
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

        /// <summary>
        /// Gets the drive this node belongs to.
        /// </summary>
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

        /// <summary>
        /// Gets the root directory from this node.
        /// </summary>
        public XeFileSystemNode GetRoot()
        {
            var node = this;

            while (node.Parent != null)
                node = node.Parent;

            return node;
        }

        /// <summary>
        /// Gets the total number of nodes from this directory.
        /// </summary>
        /// <param name="in_isRecursiveNodes">Determines whether all subnodes from this directory will be accumulated in the total.</param>
        /// <param name="in_isFoldersIncluded">Determines whether folders are included in the total number of nodes.</param>
        public long GetTotalNodes(bool in_isRecursiveNodes = false, bool in_isFoldersIncluded = true)
        {
            var result = 0L;

            if (Type != EXeFileSystemNodeType.Directory)
                return result;

            Refresh();

            result += in_isFoldersIncluded
                ? Nodes.Count()
                : GetTotalFiles();

            foreach (var node in Nodes)
            {
                if (node.Type == EXeFileSystemNodeType.File)
                    continue;

                if (in_isRecursiveNodes)
                {
                    node.Refresh();
                    result += node.GetTotalNodes(in_isRecursiveNodes, in_isFoldersIncluded);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the total number of file nodes from this directory.
        /// </summary>
        public long GetTotalFiles()
        {
            return Nodes.Count(x => x.Type == EXeFileSystemNodeType.File);
        }

        /// <summary>
        /// Gets the total number of directory nodes from this directory.
        /// </summary>
        public long GetTotalDirectories()
        {
            return Nodes.Count(x => x.Type == EXeFileSystemNodeType.Directory);
        }

        /// <summary>
        /// Gets the total size (in bytes) of all file nodes from this directory.
        /// <para>This returns <see cref="Size"/> if this node is a file.</para>
        /// </summary>
        /// <param name="in_isRecursiveNodes">Determines whether all subnodes from this directory will be accumulated in the total.</param>
        public ulong GetTotalDataSize(bool in_isRecursiveNodes = false)
        {
            var result = 0UL;

            result += Size;

            foreach (var node in Nodes)
            {
                if (node.Type == EXeFileSystemNodeType.Directory)
                {
                    if (in_isRecursiveNodes)
                    {
                        node.Refresh();
                        result += node.GetTotalDataSize(in_isRecursiveNodes);
                    }

                    continue;
                }

                result += node.Size;
            }

            return result;
        }

        /// <summary>
        /// Gets the relative path to this node after the drive name.
        /// </summary>
        /// <param name="in_depth">The depth of the path relative to the end.</param>
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

        /// <summary>
        /// Gets friendly information about this node.
        /// </summary>
        public string GetInfo(bool in_isRecursiveNodes = false)
        {
            var dataSize = GetTotalDataSize(in_isRecursiveNodes);

            var info = $"Name ────────── : {Name ?? "Unknown"}\n" +
                       $"Size ────────── : {FormatHelper.ByteLengthToDecimalString(dataSize)} ({dataSize:N0} bytes)\n" +
                       $"Date Created ── : {DateCreated:dd/MM/yyyy hh:mm tt}\n" +
                       $"Date Modified ─ : {DateModified:dd/MM/yyyy hh:mm tt}\n" +
                       $"Type ────────── : {Type}\n" +
                       $"Attributes ──── : {Attributes}";

            if (Type == EXeFileSystemNodeType.Directory)
            {
                info += $"\nFiles ───────── : {GetTotalFiles()}";
                info += $"\nDirectories ─── : {GetTotalDirectories()}";
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
