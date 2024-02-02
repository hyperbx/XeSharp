namespace XeSharp.Net.Events
{
    public class ClientWriteEventArgs(int in_bytesWritten, int in_bytesTotal) : EventArgs
    {
        public int BytesWritten { get; } = in_bytesWritten;
        public int BytesTotal { get; } = in_bytesTotal;
    }

    public delegate void ClientWriteEventHandler(object in_sender, ClientWriteEventArgs in_args);
}
