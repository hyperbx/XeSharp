namespace XeSharp.Device
{
    public class XeDbgConsoleInfo
    {
        /// <summary>
        /// The name of this console.
        /// </summary>
        public string DebugName { get; private set; }

        /// <summary>
        /// The unique identifier of this console.
        /// </summary>
        public long ID { get; private set; }

        /// <summary>
        /// Determines whether this console has 1.00 GB of memory, as opposed to the default 512 MB.
        /// </summary>
        public bool IsExtendedRAM { get; private set; }

        /// <summary>
        /// Determines whether this console has a HDD installed.
        /// </summary>
        public bool IsHDDInstalled { get; private set; }

        /// <summary>
        /// The type of unit this console is.
        /// </summary>
        public EXeConsoleType Type { get; private set; }

        /// <summary>
        /// The appearance of this console guessed by its type and memory capacity.
        /// </summary>
        public EXeConsoleAppearance GuessedAppearance { get; private set; }

        /// <summary>
        /// The platform of this console.
        /// </summary>
        public EXeConsolePlatform Platform { get; private set; }

        /// <summary>
        /// The motherboard revision used by this console.
        /// </summary>
        public EXeConsoleRevision Revision { get; private set; }

        /// <summary>
        /// The base kernel version used by this console.
        /// </summary>
        public Version BaseKernelVersion { get; private set; }

        /// <summary>
        /// The current kernel version used by this console.
        /// </summary>
        public Version KernelVersion { get; private set; }

        /// <summary>
        /// The current XDK version used by this console.
        /// </summary>
        public Version XDKVersion { get; private set; }

        public XeDbgConsoleInfo() { }

        public XeDbgConsoleInfo(XeDbgConsole in_console)
        {
            DebugName = in_console.Client.SendCommand("dbgname")?.Message ?? "Xbox 360";

            ID = long.Parse(in_console.Client.SendCommand("getconsoleid")?.Message.Split('=')[1]);

            var consoleFeatures = in_console.Client.SendCommand("consolefeatures")?.Message;

            if (!string.IsNullOrEmpty(consoleFeatures))
                IsExtendedRAM = consoleFeatures.Contains("1GB_RAM");

            var consoleType = in_console.Client.SendCommand("consoletype")?.Message;

            if (!string.IsNullOrEmpty(consoleType))
            {
                Type = Enum.Parse<EXeConsoleType>(consoleType, true);

                GuessedAppearance = Type switch
                {
                    EXeConsoleType.DevKit => EXeConsoleAppearance.Black,
                    EXeConsoleType.TestKit => EXeConsoleAppearance.White,
                    EXeConsoleType.ReviewerKit => EXeConsoleAppearance.Black,
                    _ => EXeConsoleAppearance.Blue,
                };

                if (!IsExtendedRAM)
                    GuessedAppearance |= EXeConsoleAppearance.NoSideCar;
            }

            var systemInfo = in_console.Client.SendCommand("systeminfo")?.Results as string[];

            if (systemInfo.Length > 0)
            {
                foreach (var info in systemInfo)
                {
                    var split = info.Split(['=', ' ']);

                    switch (split[0])
                    {
                        case "HDD":
                            IsHDDInstalled = split[1] == "Enabled";
                            break;

                        case "Platform":
                            Platform = Enum.Parse<EXeConsolePlatform>(split[1], true);
                            break;

                        case "BaseKrnl":
                            BaseKernelVersion = new Version(split[1]);
                            break;
                    }

                    if (split.Length <= 2)
                        continue;

                    switch (split[2])
                    {
                        case "System":
                            Revision = Enum.Parse<EXeConsoleRevision>(split[3], true);
                            break;

                        case "Krnl":
                            KernelVersion = new Version(split[3]);
                            break;
                    }

                    if (split.Length > 5 && split[4] == "XDK")
                        XDKVersion = new Version(split[5]);
                }
            }
        }

        public override string ToString()
        {
            return $"Name ---------------- : {DebugName}\n" +
                   $"ID ------------------ : {ID} (0x{ID:X16})\n" +
                   $"RAM ----------------- : {(IsExtendedRAM ? "1.00 GB" : "512 MB")}\n" +
                   $"HDD ----------------- : {(IsHDDInstalled ? "Yes" : "No")}\n" +
                   $"Type ---------------- : {Type}\n" +
                   $"Guessed Appearance -- : {GuessedAppearance}\n" +
                   $"Platform ------------ : {Platform}\n" +
                   $"Revision ------------ : {Revision}\n" +
                   $"Base Kernel Version - : {BaseKernelVersion}\n" +
                   $"Kernel Version ------ : {KernelVersion}\n" +
                   $"XDK Version --------- : {XDKVersion}";
        }
    }
}
