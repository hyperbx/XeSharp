namespace XeSharp.Device.FileSystem
{
    [Flags]
    public enum EXeFileSystemNodeAttribute
    {
        None,

        /// <summary>
        /// This node is read-only.
        /// </summary>
        ReadOnly,

        /// <summary>
        /// This node is hidden to frontends.
        /// </summary>
        Hidden
    }
}
