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

        public XeDbgClient Client { get; }
        public XeDbgConsoleInfo Info { get; private set; }
        public XeFileSystem FileSystem { get; private set; }

        public XeDbgConsole() { }

        public XeDbgConsole(string in_hostName, bool in_isClientOnly = false, bool in_isFullFileSystemMap = true)
        {
            Client = new XeDbgClient(in_hostName);

            if (in_isClientOnly)
                return;

            FinaliseCtor(in_isFullFileSystemMap);
        }

        public XeDbgConsole(XeDbgClient in_client, bool in_isClientOnly = false, bool in_isFullFileSystemMap = true)
        {
            Client = in_client;

            if (in_isClientOnly)
                return;

            FinaliseCtor(in_isFullFileSystemMap);
        }

        private void FinaliseCtor(bool in_isFullFileSystemMap)
        {
            Info = new XeDbgConsoleInfo(this);
            FileSystem = new XeFileSystem(this, in_isFullFileSystemMap);
        }

        public void Restart()
        {
            Client.SendCommand("magicboot cold");
        }

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

        public void Eject()
        {
            Client.SendCommand("dvdeject");
        }

        public byte[] ReadBytes(uint in_addr, int in_length)
        {
            var response = Client.SendCommand($"getmem addr={in_addr} length={in_length}");

            return MemoryHelper.HexStringToByteArray(string.Join(string.Empty, response.Results));
        }

        public T Read<T>(uint in_addr) where T : unmanaged
        {
            var data = ReadBytes(in_addr, Marshal.SizeOf(typeof(T)));

            if (data.Length <= 0)
                return default;

            return MemoryHelper.ByteArrayToStructure<T>(data.Reverse().ToArray());
        }

        public XeDbgResponse WriteBytes(uint in_addr, byte[] in_data)
        {
            var cmd = $"setmem addr={in_addr} data=";

            // TODO: account for commands having a 512 character limit (see XBDM_LINE_TOO_LONG).
            // var len = cmd.Length + (in_data.Length * 2);

            foreach (var b in in_data)
                cmd += b.ToString("X2");

            return Client.SendCommand(cmd);
        }

        public XeDbgResponse Write<T>(uint in_addr, T in_data) where T : unmanaged
        {
            return WriteBytes(in_addr, MemoryHelper.StructureToByteArray(in_data));
        }

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

        public XeModuleInfo GetMainModule()
        {
            var modules = GetModules();

            if (modules.Count <= 0)
                return default;

            return modules.Last().Value;
        }

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

        public List<uint> ScanSignature(byte[] in_pattern, string in_mask, string in_moduleName = "", bool in_isFirstResult = true)
        {
            return ScanSignature(null, in_pattern, in_mask, in_moduleName, in_isFirstResult);
        }
    }
}
