using XeSharp.Helpers;

namespace XeSharp.Net.Events
{
    public class ClientReadEventArgs(bool in_isBegin, int in_bytesRead, int in_bytesTotal) : EventArgs
    {
        public bool IsBegin { get; } = in_isBegin;
        public int BytesRead { get; } = in_bytesRead;
        public int BytesTotal { get; } = in_bytesTotal;
        public string BytesReadFormatted { get; } = FormatHelper.ByteLengthToDecimalString(in_bytesRead);
        public string BytesTotalFormatted { get; } = FormatHelper.ByteLengthToDecimalString(in_bytesTotal);
    }

    public delegate void ClientReadEventHandler(object in_sender, ClientReadEventArgs in_args);
}
