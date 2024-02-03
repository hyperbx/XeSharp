namespace XeSharp.Device
{
    [Flags]
    public enum EXeConsoleAppearance
    {
        Unknown = 0,

        /// <summary>
        /// This console is likely either a development kit or a reviewer kit.
        /// </summary>
        Black = 1,

        /// <summary>
        /// This console is likely an XNA development kit.
        /// </summary>
        Blue = 2,

        /// <summary>
        /// This console is likely an XNA development kit.
        /// </summary>
        BlueGrey = 4,

        /// <summary>
        /// This console does not have extended memory or hardware.
        /// </summary>
        NoSideCar = 8,

        /// <summary>
        /// This console is likely a test kit.
        /// </summary>
        White = 16
    }
}
