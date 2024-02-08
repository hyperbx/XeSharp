using XeSharp.Helpers;

namespace XeSharp.Net.Events
{
    public class ClientWriteEventArgs(bool in_isBegin, uint in_bytesWritten, uint in_bytesTotal) : EventArgs
    {
        /// <summary>
        /// Determines whether this event has just begun.
        /// </summary>
        public bool IsBegin { get; } = in_isBegin;

        /// <summary>
        /// The amount of bytes that have been written.
        /// </summary>
        public uint BytesWritten { get; } = in_bytesWritten;

        /// <summary>
        /// The total amount of bytes to write.
        /// </summary>
        public uint BytesTotal { get; } = in_bytesTotal;

        /// <summary>
        /// A formatted representation of the amount of bytes that have been written.
        /// </summary>
        public string BytesWrittenFormatted { get; } = FormatHelper.ByteLengthToDecimalString(in_bytesWritten);

        /// <summary>
        /// A formatted representation of the total amount of bytes to write.
        /// </summary>
        public string BytesTotalFormatted { get; } = FormatHelper.ByteLengthToDecimalString(in_bytesTotal);
    }

    public delegate void ClientWriteEventHandler(object in_sender, ClientWriteEventArgs in_args);
}
