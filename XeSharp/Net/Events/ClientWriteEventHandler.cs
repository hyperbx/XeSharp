using XeSharp.Helpers;

namespace XeSharp.Net.Events
{
    public class ClientWriteEventArgs(bool in_isBegin, int in_bytesWritten, int in_bytesTotal) : EventArgs
    {
        public bool IsBegin { get; } = in_isBegin;
        public int BytesWritten { get; } = in_bytesWritten;
        public int BytesTotal { get; } = in_bytesTotal;
        public string BytesWrittenFormatted { get; } = FormatHelper.ByteLengthToDecimalString(in_bytesWritten);
        public string BytesTotalFormatted { get; } = FormatHelper.ByteLengthToDecimalString(in_bytesTotal);
    }

    public delegate void ClientWriteEventHandler(object in_sender, ClientWriteEventArgs in_args);
}
