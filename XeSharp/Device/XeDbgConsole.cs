using System.Runtime.InteropServices;
using XeSharp.Device.FileSystem;
using XeSharp.Device.Title;
using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Net.Sockets;

namespace XeSharp.Device
{
    public class XeDbgConsole
    {
        private const int _maxCommandLength = 512;

        /// <summary>
        /// The client connected to this console.
        /// </summary>
        public XeDbgClient Client { get; private set; }

        /// <summary>
        /// Information about this console.
        /// </summary>
        public XeDbgConsoleInfo Info { get; private set; }

        /// <summary>
        /// This console's filesystem.
        /// </summary>
        public XeFileSystem FileSystem { get; private set; }

        public XeDbgConsole() { }

        /// <summary>
        /// Connects to a console via a pre-existing client.
        /// </summary>
        /// <param name="in_client">The client to connect to the console.</param>
        /// <param name="in_isClientOnly">Determines whether only the client will be initialised.</param>
        /// <param name="in_isFullFileSystemMap">Determines whether the full filesystem will be mapped.</param>
        public XeDbgConsole(XeDbgClient in_client, bool in_isClientOnly = false, bool in_isFullFileSystemMap = true)
        {
            Client = in_client;

            if (in_isClientOnly)
                return;

            Info = new XeDbgConsoleInfo(this);
            FileSystem = new XeFileSystem(this, in_isFullFileSystemMap);
        }

        /// <summary>
        /// Connects to a console via its host name or IP address.
        /// </summary>
        /// <param name="in_hostName">The host name or IP address of the console.</param>
        /// <param name="in_isClientOnly">Determines whether only the client will be initialised.</param>
        /// <param name="in_isFullFileSystemMap">Determines whether the full filesystem will be mapped.</param>
        public XeDbgConsole(string in_hostName, bool in_isClientOnly = false, bool in_isFullFileSystemMap = true)
            : this(new XeDbgClient(in_hostName), in_isClientOnly, in_isFullFileSystemMap) { }

        /// <summary>
        /// Restarts the console.
        /// </summary>
        public void Restart()
        {
            Client.SendCommand("magicboot cold");
        }

        /// <summary>
        /// Launches a remote executable binary.
        /// </summary>
        /// <param name="in_path">The path to the executable.</param>
        /// <param name="in_args">The command line arguments to pass to the executable.</param>
        /// <param name="in_bootType">Determines how this executable should be launched.</param>
        public void Launch(string in_path, string in_args = "", EXeBootType in_bootType = EXeBootType.Title)
        {
            var cmd = "magicboot";

            if (in_bootType != EXeBootType.Title)
                cmd += $" {in_bootType.ToString().ToLower()}";

            if (!string.IsNullOrEmpty(in_path))
                cmd += $" title=\"{FileSystem.GetNodeFromPath(in_path)}\"";

            if (!string.IsNullOrEmpty(in_args))
                cmd += $" cmdline=\"{in_args}\"";

            Client.SendCommand(cmd);
        }

        /// <summary>
        /// Ejects the DVD drive on the console.
        /// </summary>
        public void Eject()
        {
            Client.SendCommand("dvdeject");
        }

        /// <summary>
        /// Reads a memory location into a buffer.
        /// </summary>
        /// <param name="in_addr">The virtual address to read from.</param>
        /// <param name="in_length">The length of memory to read.</param>
        public byte[] ReadBytes(uint in_addr, int in_length)
        {
            var response = Client.SendCommand($"getmem addr={in_addr} length={in_length}");

            return MemoryHelper.HexStringToByteArray(string.Join(string.Empty, response.Results));
        }

        /// <summary>
        /// Reads a memory location into an unmanaged type.
        /// </summary>
        /// <typeparam name="T">The type to implicitly cast to.</typeparam>
        /// <param name="in_addr">The virtual address to read from.</param>
        public T Read<T>(uint in_addr) where T : unmanaged
        {
            var data = ReadBytes(in_addr, Marshal.SizeOf(typeof(T)));

            if (data.Length <= 0)
                return default;

            return MemoryHelper.ByteArrayToStructure<T>(data.Reverse().ToArray());
        }

        /// <summary>
        /// Writes a buffer to a memory location.
        /// </summary>
        /// <param name="in_addr">The virtual address to write to.</param>
        /// <param name="in_data">The buffer to write.</param>
        public XeDbgResponse WriteBytes(uint in_addr, byte[] in_data)
        {
            var cmd = $"setmem addr={in_addr} data=";

            // TODO: account for commands having a 512 character limit (see XBDM_LINE_TOO_LONG).
            // var len = cmd.Length + (in_data.Length * 2);

            foreach (var b in in_data)
                cmd += b.ToString("X2");

            return Client.SendCommand(cmd);
        }

        /// <summary>
        /// Writes an unmanaged type to a memory location.
        /// </summary>
        /// <typeparam name="T">The implicit type to write.</typeparam>
        /// <param name="in_addr">The virtual address to write to.</param>
        /// <param name="in_data">The data to write.</param>
        public XeDbgResponse Write<T>(uint in_addr, T in_data) where T : unmanaged
        {
            return WriteBytes(in_addr, MemoryHelper.StructureToByteArray(in_data));
        }

        /// <summary>
        /// Gets all modules.
        /// </summary>
        public Dictionary<string, XeModuleInfo> GetModules()
        {
            var result = new Dictionary<string, XeModuleInfo>();
            var moduleCsvs = Client.SendCommand("modules")?.Results as string[];

            if (moduleCsvs?.Length <= 0)
                return result;

            foreach (var moduleCsv in moduleCsvs)
            {
                var moduleInfo = new XeModuleInfo(moduleCsv);
                result.Add(moduleInfo.Name, moduleInfo);
            }

            return result;
        }

        /// <summary>
        /// Gets the current foreground module.
        /// </summary>
        public XeModuleInfo GetForegroundModule()
        {
            var modules = GetModules();

            if (modules.Count <= 0)
                return default;

            return modules.Last().Value;
        }

        /// <summary>
        /// Scans the current foreground module for a byte pattern.
        /// </summary>
        /// <param name="in_memory">The buffer to scan (downloads the current foreground module if null).</param>
        /// <param name="in_pattern">The pattern to scan for.</param>
        /// <param name="in_mask">The mask of the pattern ('x' for scannable bytes, '?' for any byte).</param>
        /// <param name="in_moduleName">The name of the module to scan (used to get the address and size to scan).</param>
        /// <param name="in_isFirstResult">Determines whether this function returns the first match it finds, rather than all matches.</param>
        public List<uint> ScanSignature(byte[] in_memory, byte[] in_pattern, string in_mask, string in_moduleName = "", bool in_isFirstResult = true)
        {
            var results = new List<uint>();

            if (in_pattern.Length <= 0)
                return results;

            var modules = GetModules();

            if (!string.IsNullOrEmpty(in_moduleName) && !modules.ContainsKey(in_moduleName))
                return results;

            var module = string.IsNullOrEmpty(in_moduleName)
                ? modules.Last().Value
                : modules[in_moduleName];

            /* TODO: this could (and probably should) be optimised to scan
                     in chunks, rather than downloading the entire binary. */
            var memory = in_memory ?? ReadBytes(module.BaseAddress, (int)module.ImageSize);

            for (uint i = 0; i < memory.Length; i++)
            {
                int sigIndex;

                for (sigIndex = 0; sigIndex < in_mask.Length; sigIndex++)
                {
                    if (memory.Length <= i + sigIndex)
                        break;

                    if (in_mask[sigIndex] != '?' && in_pattern[sigIndex] != memory[i + sigIndex])
                        break;
                }

                if (sigIndex == in_mask.Length)
                {
                    results.Add(module.BaseAddress + i);

                    if (in_isFirstResult)
                        return results;
                }
            }

            return results;
        }

        /// <summary>
        /// Scans the current foreground module for a byte pattern.
        /// </summary>
        /// <param name="in_pattern">The pattern to scan for.</param>
        /// <param name="in_mask">The mask of the pattern ('x' for scannable bytes, '?' for any byte).</param>
        /// <param name="in_moduleName">The name of the module to scan (used to get the address and size to scan).</param>
        /// <param name="in_isFirstResult">Determines whether this function returns the first match it finds, rather than all matches.</param>
        public List<uint> ScanSignature(byte[] in_pattern, string in_mask, string in_moduleName = "", bool in_isFirstResult = true)
        {
            return ScanSignature(null, in_pattern, in_mask, in_moduleName, in_isFirstResult);
        }

        public override string ToString()
        {
            return (Info?.DebugName ?? "Xbox 360") + $" @ \"{Client.HostName}\"";
        }
    }
}
