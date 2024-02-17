using XeSharp.Device.FileSystem;
using XeSharp.Device.Memory;
using XeSharp.Device.Title;
using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Net.Sockets;

namespace XeSharp.Device
{
    public class XeConsole
    {
        private const int _maxCommandLength = 512;

        /// <summary>
        /// The default thread ID used by the foreground module.
        /// </summary>
        public const int MainThreadID = -117440512; // 0xF9000000

        /// <summary>
        /// The client connected to this console.
        /// </summary>
        public XeClient Client { get; private set; }

        /// <summary>
        /// Information about this console.
        /// </summary>
        public XeConsoleInfo Info { get; private set; }

        /// <summary>
        /// This console's filesystem.
        /// </summary>
        public XeFileSystem FileSystem { get; private set; }

        /// <summary>
        /// This console's memory.
        /// </summary>
        public XeMemory Memory { get; private set; }

        public XeConsole() { }

        /// <summary>
        /// Connects to a console via a pre-existing client.
        /// </summary>
        /// <param name="in_client">The client to connect to the console.</param>
        /// <param name="in_isClientOnly">Determines whether only the client will be initialised.</param>
        /// <param name="in_isFullFileSystemMap">Determines whether the full filesystem will be mapped.</param>
        public XeConsole(XeClient in_client, bool in_isClientOnly = false, bool in_isFullFileSystemMap = true)
        {
            Client = in_client;

            if (in_isClientOnly)
                return;

            Info = new XeConsoleInfo(this);
            FileSystem = new XeFileSystem(this, in_isFullFileSystemMap);
        }

        /// <summary>
        /// Connects to a console via its host name or IP address.
        /// </summary>
        /// <param name="in_hostName">The host name or IP address of the console.</param>
        /// <param name="in_isClientOnly">Determines whether only the client will be initialised.</param>
        /// <param name="in_isFullFileSystemMap">Determines whether the full filesystem will be mapped.</param>
        public XeConsole(string in_hostName, bool in_isClientOnly = false, bool in_isFullFileSystemMap = true)
            : this(new XeClient(in_hostName), in_isClientOnly, in_isFullFileSystemMap) { }

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
        public XeModuleInfo GetMainModule()
        {
            var modules = GetModules();

            if (modules.Count <= 0)
                return default;

            return modules.Last().Value;
        }

        /// <summary>
        /// Gets all threads.
        /// </summary>
        public Dictionary<int, XeThreadInfo> GetThreads()
        {
            var result = new Dictionary<int, XeThreadInfo>();
            var threadIDs = Client.SendCommand("threads");

            if (threadIDs.Status.ToHResult() != EXeStatusCode.XBDM_MULTIRESPONSE)
                return [];

            foreach (var thread in (string[])threadIDs.Results)
            {
                var threadID = MemoryHelper.ChangeType<int>(thread);

                result.Add(threadID, new XeThreadInfo(this, threadID));
            }

            return result;
        }

        /// <summary>
        /// Gets the current thread.
        /// </summary>
        public XeThreadInfo GetMainThread()
        {
            var threads = GetThreads();

            if (threads.Count <= 0)
                return default;

            return threads[MainThreadID];
        }

        public override string ToString()
        {
            return (Info?.DebugName ?? "Xbox 360") + $" @ \"{Client.HostName}\"";
        }
    }
}
