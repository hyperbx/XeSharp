namespace XeSharp.Device
{
    [Flags]
    public enum EXeConsoleAppearance
    {
        Unknown = 0,

        /// <summary>
        /// This console is likely a development kit.
        /// </summary>
        Black = 1,

        /// <summary>
        /// This console is likely a development kit with extended memory.
        /// </summary>
        Blue = 2,

        /// <summary>
        /// This console is likely a test kit with extended memory.
        /// </summary>
        BlueGrey = 4,

        /// <summary>
        /// This console does not have extended memory or hardware.
        /// </summary>
        NoSideCar = 8,

        /// <summary>
        /// This console is likely a retail, demo or test kit.
        /// <para>If this console has a side car, it is a test kit.</para>
        /// </summary>
        White = 16
    }
}
