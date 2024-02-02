namespace XeSharp.Net.Events
{
    public class ClientReadEventArgs(int in_bytesRead, int in_bytesTotal) : EventArgs
    {
        public int BytesRead { get; } = in_bytesRead;
        public int BytesTotal { get; } = in_bytesTotal;
    }

    public delegate void ClientReadEventHandler(object in_sender, ClientReadEventArgs in_args);
}
