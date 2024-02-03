namespace XeSharp.Device
{
    public enum EXeBootType
    {
        /// <summary>
        /// This option boots into a title normally.
        /// </summary>
        Title,

        /// <summary>
        /// This option boots into a title and halts execution until resumed.
        /// </summary>
        Wait,

        /// <summary>
        /// This option is deprecated.
        /// </summary>
        Warm,

        /// <summary>
        /// This option restarts the console.
        /// </summary>
        Cold,

        /// <summary>
        /// This option boots into a title and halts execution until resumed.
        /// </summary>
        Stop
    }
}
