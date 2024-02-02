using System.Net.Sockets;
using System.Text;
using XeSharp.Net.Events;

namespace XeSharp.Net.Sockets
{
    public class XeDbgClient : IDisposable
    {
        internal TcpClient Client;
        internal StreamReader Reader;
        internal BinaryWriter Writer;

        public event ClientReadEventHandler ReadEvent;
        public event ClientWriteEventHandler WriteEvent;

        public XeDbgClientInfo Info { get; private set; }

        public string HostName { get; private set; } = "0.0.0.0";

        public XeDbgClient() { }

        public XeDbgClient(string in_hostName)
        {
            Connect(in_hostName);
        }

        public void Connect(string in_hostName)
        {
            // Console is already connected.
            if (in_hostName == HostName && IsConnected())
                return;

            HostName = in_hostName;

            Client = new TcpClient(in_hostName, 730);
            Reader = new StreamReader(Client.GetStream());
            Writer = new BinaryWriter(Client.GetStream());

            // Flush connected message.
            Pop();

            Info = new XeDbgClientInfo(this);
        }

        public void Disconnect()
        {
            SendCommand("bye");
            Dispose();
        }

        public bool IsConnected()
        {
            // FIXME: this doesn't always return false when the console is shut down.
            return Client != null && Client.Connected;
        }

        public string Pop()
        {
            if (!IsConnected())
                return string.Empty;

            return Reader.ReadLine();
        }

        public XeDbgResponse SendCommand(string in_command, bool in_isThrowExceptionOnServerError = true)
        {
            if (!IsConnected())
                return null;

            Writer?.Write(Encoding.ASCII.GetBytes($"{in_command}\r\n"));

            return GetResponse(in_isThrowExceptionOnServerError);
        }

        public XeDbgResponse GetResponse(bool in_isThrowExceptionOnServerError = true)
        {
            if (!IsConnected())
                return null;

            var response = new XeDbgResponse(this);

            if (in_isThrowExceptionOnServerError && response.Status.IsFailed())
                throw new Exception(response.Status.ToString());

            if (response.Message?.Equals("bye", StringComparison.CurrentCultureIgnoreCase) ?? false)
                Dispose();

            return response;
        }

        public string[] ReadLines()
        {
            var result = new List<string>();

            while (true)
            {
                var line = Reader.ReadLine();

                if (line == "." || XeDbgResponse.IsStatusMessage(line))
                    break;

                result.Add(line);
            }

            return [.. result];
        }

        public byte[] ReadBytes()
        {
            var stream = Client.GetStream();

            byte[] sizeBuffer = new byte[4];
            stream.Read(sizeBuffer, 0, sizeBuffer.Length);

            byte[] buffer = new byte[BitConverter.ToInt32(sizeBuffer)];
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public void WriteBytes(byte[] in_data)
        {
            WriteBytesAsync(in_data).GetAwaiter().GetResult();
        }

        public async Task WriteBytesAsync(byte[] in_data)
        {
            if (in_data.Length > int.MaxValue)
                throw new InvalidDataException($"The input buffer is too large ({int.MaxValue:N0} bytes max).");

            var bytesSent = 0;
            var bytesTotal = in_data.Length;

            while (bytesSent < bytesTotal)
            {
                int chunkSize = Math.Min(0x1000, bytesTotal - bytesSent);

                byte[] chunk = new byte[chunkSize];
                Buffer.BlockCopy(in_data, bytesSent, chunk, 0, chunkSize);

                await Client.GetStream().WriteAsync(chunk);

                bytesSent += chunkSize;

                OnWrite(new ClientWriteEventArgs(bytesSent, bytesTotal));
            }
        }

        protected virtual void OnRead(ClientReadEventArgs in_args)
        {
            ReadEvent?.Invoke(this, in_args);
        }

        protected virtual void OnWrite(ClientWriteEventArgs in_args)
        {
            WriteEvent?.Invoke(this, in_args);
        }

        public void Dispose()
        {
            Client?.Close();
            Reader?.Dispose();
            Writer?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
