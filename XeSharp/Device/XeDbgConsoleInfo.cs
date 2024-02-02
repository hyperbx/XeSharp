namespace XeSharp.Device
{
    public class XeDbgConsoleInfo
    {
        public string DebugName { get; set; }
        public long ID { get; set; }
        public bool IsExtendedRAM { get; set; }
        public bool IsHDDInstalled { get; set; }
        public EXeConsoleType Type { get; set; }
        public EXeConsoleAppearance GuessedAppearance { get; set; }
        public EXeConsolePlatform Platform { get; set; }
        public EXeConsoleRevision Revision { get; set; }
        public Version BaseKernelVersion { get; set; }
        public Version KernelVersion { get; set; }
        public Version XDKVersion { get; set; }

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
