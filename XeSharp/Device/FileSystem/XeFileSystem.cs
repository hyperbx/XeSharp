using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.FileSystem
{
    public class XeFileSystem
    {
        protected XeDbgConsole _console;

        public XeFileSystemNode CurrentDirectory { get; set; }

        public XeFileSystem() { }

        public XeFileSystem(XeDbgConsole in_console, bool in_isFullFileSystemMap = true)
        {
            _console = in_console;

            // Initialise root node.
            CurrentDirectory = GetDrivesRoot(in_isFullFileSystemMap);
        }

        public string ToAbsolutePath(string in_path)
        {
            if (string.IsNullOrEmpty(in_path))
                return in_path;

            var result = FormatHelper.IsAbsolutePath(in_path)
                ? in_path
                : Path.Combine(CurrentDirectory.ToString(), Path.GetDirectoryName(in_path));

            if (result?.EndsWith(':') == true)
                result += '\\';

            return result ?? in_path;
        }

        public bool FileExists(string in_path)
        {
            var response = _console.Client.SendCommand($"getfileattributes name=\"{GetNodeFromPath(in_path)}\"", false);

            return response.Status.ToHResult() != EXeDbgStatusCode.XBDM_NOSUCHFILE;
        }

        public static byte[] Download(XeDbgConsole in_console, string in_path)
        {
            var response = in_console.Client.SendCommand($"getfile name=\"{in_path}\"", false);

            // File does not exist.
            if (response.Status.ToHResult() == EXeDbgStatusCode.XBDM_MEMUNMAPPED)
                return [];

            return response?.Results.Cast<byte>().ToArray();
        }

        public byte[] Download(string in_path)
        {
            return Download(_console, in_path);
        }

        public List<XeFileSystemDrive> GetDrives(bool in_isRecursiveNodes = true)
        {
            var result = new List<XeFileSystemDrive>();

            // Map flash memory in drive list.
            _console.Client.SendCommand("drivemap internal");

            var drives = _console.Client.SendCommand("drivelist", false)?.Results as string[];

            if (drives == null || drives.Length <= 0)
                return result;

            var ini = IniParser.DoLines(drives);

            if (ini.Count <= 0)
                return result;

            foreach (var driveNameKey in ini[""].Keys)
            {
                var driveName = ini[""][driveNameKey] + ':';

                var drive = new XeFileSystemDrive(_console, driveName);
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

        public XeFileSystemNode GetDrivesRoot(bool in_isRecursiveNodes = true)
        {
            var result = new XeFileSystemNode()
            {
                Type = EXeFileSystemNodeType.Directory,
                Attributes = EXeFileSystemNodeAttribute.Readonly
            };

            var drives = GetDrives(in_isRecursiveNodes);

            for (int i = 0; i < drives.Count; i++)
                drives[i].Parent = result;

            result.Nodes = drives;

            return result;
        }

        public List<XeFileSystemNode> GetNodesFromPath(string in_path, bool in_isRecursiveNodes = true, XeFileSystemNode in_parent = null)
        {
            var result = new List<XeFileSystemNode>();
            var path = ToAbsolutePath(in_path);
            var nodeCsvs = _console.Client.SendCommand($"dirlist name=\"{path}\"", false)?.Results as string[];

            if (nodeCsvs == null || nodeCsvs.Length <= 0)
                return result;

            foreach (var nodeCsv in nodeCsvs)
            {
                var node = new XeFileSystemNode(nodeCsv);

                if (in_parent != null)
                {
                    node.Drive  = in_parent.Drive ?? in_parent.GetDrive();
                    node.Parent = in_parent;
                }

                if (in_isRecursiveNodes && node.Type == EXeFileSystemNodeType.Directory)
                    node.Nodes = GetNodesFromPath(Path.Combine(path, node.Name), in_parent: node);

                result.Add(node);
            }

            // Sort alphanumerically.
            result.Sort((x, y) => x.Name.CompareTo(y.Name));

            return result;
        }

        public XeFileSystemNode GetNodeFromPath(string in_path)
        {
            if (string.IsNullOrEmpty(in_path))
                return null;

            var dir = GetDirectoryFromPath(ToAbsolutePath(in_path));
            var fileName = Path.GetFileName(in_path);

            if (dir == null || !dir.Nodes.Any())
                return null;

            foreach (var node in dir.Nodes)
            {
                // Paths are case-insensitive.
                if (node.Name.ToLower() == fileName.ToLower())
                    return node;
            }

            return null;
        }

        public XeFileSystemNode GetDirectoryFromPath(string in_path, XeFileSystemNode in_node = null)
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

        WalkNodes:
            for (int i = 0; i < dir?.Nodes.Count(); i++)
            {
                var node = dir.Nodes.ElementAt(i);

                if (node.Type != EXeFileSystemNodeType.Directory)
                    continue;

                // Refresh nodes in current directory.
                node.Nodes = GetNodesFromPath(node.ToString(), false, node);

                for (int j = 0; j < paths.Count; j++)
                {
                    var path = paths[j];

                    // Visit parent directory of current node.
                    if (path == ".." && dir.Parent != null)
                    {
                        dir = dir.Parent;
                        paths.RemoveAt(j);
                        goto WalkNodes;
                    }

                    // Paths are case-insensitive.
                    if (node.Name.ToLower() != path.ToLower())
                        continue;

                    if (paths.Count == 1)
                        return node;

                    dir = GetDirectoryFromPath(string.Join('\\', paths.Skip(1)), node);
                }
            }

            return dir;
        }

        public XeFileSystemNode ChangeDirectory(string in_path)
        {
            return CurrentDirectory = GetDirectoryFromPath(in_path);
        }
    }
}
