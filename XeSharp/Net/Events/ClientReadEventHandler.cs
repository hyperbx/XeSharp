using XeSharp.Helpers;

namespace XeSharp.Net.Events
{
    public class ClientReadEventArgs(bool in_isBegin, int in_bytesRead, int in_bytesTotal) : EventArgs
    {
        /// <summary>
        /// Determines whether this event has just begun.
        /// </summary>
        public bool IsBegin { get; } = in_isBegin;

        /// <summary>
        /// The amount of bytes read.
        /// </summary>
        public int BytesRead { get; } = in_bytesRead;

        /// <summary>
        /// The total amount of bytes to read.
        /// </summary>
        public int BytesTotal { get; } = in_bytesTotal;

        /// <summary>
        /// A formatted representation of the amount of bytes read.
        /// </summary>
        public string BytesReadFormatted { get; } = FormatHelper.ByteLengthToDecimalString(in_bytesRead);

        /// <summary>
        /// A formatted representation of the total amount of bytes to read.
        /// </summary>
        public string BytesTotalFormatted { get; } = FormatHelper.ByteLengthToDecimalString(in_bytesTotal);
    }

    public delegate void ClientReadEventHandler(object in_sender, ClientReadEventArgs in_args);
}
