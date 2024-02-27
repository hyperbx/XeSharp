using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.FileSystem
{
    public class XeFileSystem
    {
        internal XeConsole Console;

        /// <summary>
        /// The current navigated directory.
        /// </summary>
        public XeFileSystemNode CurrentDirectory { get; set; }

        /// <summary>
        /// Creates a new filesystem instance.
        /// </summary>
        /// <param name="in_console">The console to load the filesystem from.</param>
        /// <param name="in_isFullFileSystemMapped">Determines whether the entire filesystem will be mapped in this instance.</param>
        public XeFileSystem(XeConsole in_console, bool in_isFullFileSystemMapped = true)
        {
            Console = in_console;

            // Initialise root node.
            CurrentDirectory = GetDrivesRoot(in_isRecursiveNodes: in_isFullFileSystemMapped);
        }

        /// <summary>
        /// Transforms a relative path into an absolute path.
        /// </summary>
        /// <param name="in_path">The relative path to transform.</param>
        public string ToAbsolutePath(string in_path)
        {
            if (string.IsNullOrEmpty(in_path))
                return in_path;

            var result = FormatHelper.IsAbsolutePath(in_path)
                ? in_path
                : Path.Combine(CurrentDirectory.ToString(), in_path);

            if (result?.EndsWith(':') == true)
                result += '\\';

            return result ?? in_path;
        }

        /// <summary>
        /// Determines whether flash memory is mapped as a drive.
        /// </summary>
        public bool IsFlashMemoryMapped()
        {
            return GetDrives(false, false).Any(x => x.Name == "FLASH:");
        }

        /// <summary>
        /// Determines whether the specified file or directory exists.
        /// </summary>
        /// <param name="in_path">The path to check.</param>
        public bool Exists(string in_path)
        {
            var response = Console.Client.SendCommand($"getfileattributes name=\"{ToAbsolutePath(in_path)}\"", false);

            return response.Status.ToHResult() != EXeStatusCode.XBDM_NOSUCHFILE;
        }

        /// <summary>
        /// Deletes a remote file or directory.
        /// </summary>
        /// <param name="in_path">The path to the file or directory to delete.</param>
        public XeResponse Delete(string in_path, EXeFileSystemNodeType in_type = EXeFileSystemNodeType.File)
        {
            var cmd = $"delete name=\"{ToAbsolutePath(in_path)}\"";

            if (in_type == EXeFileSystemNodeType.Directory)
                cmd += " dir";

            return Console.Client.SendCommand(cmd, false);
        }

        /// <summary>
        /// Downloads the contents of a file into the input stream.
        /// </summary>
        /// <param name="in_console">The console containing the file.</param>
        /// <param name="in_path">The path to the file to download.</param>
        /// <param name="in_stream">The stream to write the file's data to.</param>
        public void Download(string in_path, Stream in_stream)
        {
            var response = Console.Client.SendCommand($"getfile name=\"{in_path}\"", false);

            if (response.Status.ToHResult() != EXeStatusCode.XBDM_BINRESPONSE)
                return;

            Console.Client.CopyTo(in_stream);
        }

        /// <summary>
        /// Downloads the contents of a file into a buffer.
        /// <para>Not recommended for large files.</para>
        /// </summary>
        /// <param name="in_path">The path to the file to download.</param>
        public byte[] Download(string in_path)
        {
            var response = Console.Client.SendCommand($"getfile name=\"{in_path}\"", false);

            if (response.Status.ToHResult() != EXeStatusCode.XBDM_BINRESPONSE)
                return [];

            return Console.Client.ReadBytes();
        }

        /// <summary>
        /// Uploads data to a file on the console.
        /// </summary>
        /// <param name="in_data">The data to write.</param>
        /// <param name="in_destination">The remote path to write to.</param>
        /// <param name="in_isOverwrite">Determines whether the remote file can be overwritten if it already exists.</param>
        public void Upload(byte[] in_data, string in_destination, bool in_isOverwrite = true)
        {
            ArgumentException.ThrowIfNullOrEmpty(in_destination);

            if (!in_isOverwrite && Exists(in_destination))
                throw new IOException("The destination file already exists.");

            CreateDirectory(Path.GetDirectoryName(in_destination));

            var response = Console.Client.SendCommand(
                $"sendfile name=\"{ToAbsolutePath(in_destination)}\" length={in_data.Length}");

            if (response.Status.ToHResult() != EXeStatusCode.XBDM_READYFORBIN)
                throw new IOException("An internal error occurred and the data could not be sent.");

            // TODO: stream?
            Console.Client.WriteBytes(in_data);
            Console.Client.Pop();
        }

        /// <summary>
        /// Uploads a local file to the console.
        /// </summary>
        /// <param name="in_source">The local file to upload.</param>
        /// <param name="in_destination">The remote path to write to.</param>
        /// <param name="in_isOverwrite">Determines whether the remote file can be overwritten if it already exists.</param>
        public void Upload(string in_source, string in_destination, bool in_isOverwrite = true)
        {
            ArgumentException.ThrowIfNullOrEmpty(in_source);

            if (!File.Exists(in_source))
                return;

            Upload(File.ReadAllBytes(in_source), in_destination, in_isOverwrite);
        }

        /// <summary>
        /// Copies a remote file to a remote destination.
        /// </summary>
        /// <param name="in_source">The remote path to copy from.</param>
        /// <param name="in_destination">The remote path to copy to.</param>
        /// <param name="in_isOverwrite">Determines whether the remote file can be overwritten if it already exists.</param>
        public void Copy(string in_source, string in_destination, bool in_isOverwrite = true)
        {
            ArgumentException.ThrowIfNullOrEmpty(in_source);
            ArgumentException.ThrowIfNullOrEmpty(in_destination);

            if (!Exists(in_source))
                throw new FileNotFoundException(in_source);

            if (!in_isOverwrite && Exists(in_destination))
                throw new IOException("The destination file already exists.");

            // TODO
        }

        /// <summary>
        /// Creates a directory node at the specified location.
        /// </summary>
        /// <param name="in_path">The path to create a directory node at.</param>
        public XeFileSystemNode CreateDirectory(string in_path)
        {
            ArgumentException.ThrowIfNullOrEmpty(in_path);

            var path = ToAbsolutePath(in_path);
            var pathConcat = string.Empty;

            XeResponse response = null;

            foreach (var dir in path.Split('\\'))
            {
                pathConcat += $"{dir}\\";

                // Create paths sequentially.
                response = Console.Client.SendCommand($"mkdir name=\"{pathConcat}\"", false);
            }

            if (response?.Status.ToHResult() != EXeStatusCode.XBDM_NOERR)
                return null;

            return GetNodeFromPath(path);
        }

        /// <summary>
        /// Gets all logical drives on the console.
        /// </summary>
        /// <param name="in_isFlashMemoryMapped">Determines whether flash memory will be mapped in the list (only applies once per session).</param>
        /// <param name="in_isRecursiveNodes">Determines whether all nodes in each drive will be loaded.</param>
        public List<XeFileSystemDrive> GetDrives(bool in_isFlashMemoryMapped = true, bool in_isRecursiveNodes = true)
        {
            var result = new List<XeFileSystemDrive>();

            if (in_isFlashMemoryMapped)
            {
                // Map flash memory in drive list.
                Console.Client.SendCommand("drivemap internal");
            }

            var drives = Console.Client.SendCommand("drivelist", false)?.Results as string[];

            if (drives == null || drives.Length <= 0)
                return result;

            var ini = IniParser.DoLines(drives);

            if (ini.Count <= 0)
                return result;

            foreach (var driveNameKey in ini[""].Keys)
            {
                var driveName = ini[""][driveNameKey] + ':';

                var drive = new XeFileSystemDrive(this, driveName);
                {
                    drive.Nodes = in_isRecursiveNodes
                        ? GetNodesFromPath($@"{driveName}\", in_parent: drive)
                        : [];
                }

                result.Add(drive);
            }

            // Sort alphanumerically.
            result.Sort((x, y) => x.Name.CompareTo(y.Name));

            return result;
        }

        /// <summary>
        /// Gets all logical drives on the console in a root node.
        /// </summary>
        /// <param name="in_isFlashMemoryMapped">Determines whether flash memory will be mapped in the list (only applies once per session).</param>
        /// <param name="in_isRecursiveNodes">Determines whether all nodes in each drive will be loaded.</param>
        public XeFileSystemNode GetDrivesRoot(bool in_isFlashMemoryMapped = true, bool in_isRecursiveNodes = true)
        {
            var result = new XeFileSystemNode()
            {
                Type = EXeFileSystemNodeType.Directory,
                Attributes = EXeFileSystemNodeAttribute.ReadOnly,
                IsRoot = true,
                FileSystem = this
            };

            var drives = GetDrives(in_isFlashMemoryMapped, in_isRecursiveNodes);

            for (int i = 0; i < drives.Count; i++)
                drives[i].Parent = result;

            result.Nodes = drives;

            return result;
        }

        /// <summary>
        /// Gets all nodes from the input path.
        /// </summary>
        /// <param name="in_path">The path to retrieve nodes from.</param>
        /// <param name="in_isRecursiveNodes">Determines whether all subnodes in each node will be loaded.</param>
        /// <param name="in_parent">The parent to attach to these nodes.</param>
        public List<XeFileSystemNode> GetNodesFromPath(string in_path, bool in_isRecursiveNodes = true, XeFileSystemNode in_parent = null)
        {
            var result = new List<XeFileSystemNode>();
            var path = ToAbsolutePath(in_path);
            var nodeCsvs = Console.Client.SendCommand($"dirlist name=\"{path}\"", false)?.Results as string[];

            if (nodeCsvs == null || nodeCsvs.Length <= 0)
                return result;

            foreach (var nodeCsv in nodeCsvs)
            {
                var node = new XeFileSystemNode(this, nodeCsv);

                if (in_parent != null)
                {
                    node.Drive  = in_parent.Drive ?? in_parent.GetDrive();
                    node.Parent = in_parent;

                    if (in_parent is XeFileSystemDrive drive)
                        node.Drive = drive;
                }

                if (in_isRecursiveNodes && node.Type == EXeFileSystemNodeType.Directory)
                    node.Nodes = GetNodesFromPath(Path.Combine(path, node.Name), in_parent: node);

                result.Add(node);
            }

            // Sort alphanumerically.
            result.Sort((x, y) => x.Name.CompareTo(y.Name));

            return result;
        }

        /// <summary>
        /// Gets a node from the input path.
        /// </summary>
        /// <param name="in_path">The expected path for the node.</param>
        /// <param name="in_node">The directory node to search in.</param>
        public XeFileSystemNode GetNodeFromPath(string in_path, XeFileSystemNode in_node = null)
        {
            if (string.IsNullOrEmpty(in_path))
                return null;

            if (in_node != null && in_node.Type != EXeFileSystemNodeType.Directory)
                throw new NotSupportedException("The input node must be a directory.");

            var paths = in_path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var dir = in_node ?? CurrentDirectory;

            if (dir == null)
                return null;

            // Go to root directory.
            if (FormatHelper.IsAbsolutePath(paths))
                dir = dir.GetRoot();

            // Go to parent directory.
            if (in_path == "..")
                return dir.Parent ?? dir;

            for (int i = 0; i < dir?.Nodes.Count(); i++)
            {
                var node = dir.Nodes.ElementAt(i);

                if (node.Type == EXeFileSystemNodeType.Directory)
                    node.Refresh();

                for (int j = 0; j < paths.Count; j++)
                {
                    var path = paths[j];

                    // Visit parent directory of current node.
                    if (path == ".." && dir.Parent != null)
                    {
                        dir = dir.Parent;
                        paths.RemoveAt(j);
                        break;
                    }

                    // Paths are case-insensitive.
                    if (node.Name.ToLower() != path.ToLower())
                        continue;

                    if (paths.Count == 1)
                        return node;

                    if (node.Type == EXeFileSystemNodeType.Directory)
                        dir = GetNodeFromPath(string.Join('\\', paths.Skip(1)), node);
                }
            }

            return dir;
        }

        /// <summary>
        /// Changes the current directory to the input path.
        /// </summary>
        /// <param name="in_path">The path to change to.</param>
        public XeFileSystemNode ChangeDirectory(string in_path)
        {
            return CurrentDirectory = GetNodeFromPath(in_path);
        }
    }
}
