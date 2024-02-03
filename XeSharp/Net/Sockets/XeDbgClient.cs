using System.Buffers;
using System.Net.Sockets;
using System.Text;
using XeSharp.Net.Events;

namespace XeSharp.Net.Sockets
{
    public class XeDbgClient : IDisposable
    {
        internal TcpClient Client { get; private set; }
        internal StreamReader Reader { get; private set; }
        internal BinaryWriter Writer { get; private set; }

        public XeDbgClientInfo Info { get; private set; }

        public XeDbgResponse Response { get; private set; }

        public event ClientReadEventHandler ReadEvent;
        public event ClientWriteEventHandler WriteEvent;

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

            return Response = response;
        }

        #region Reading Methods

        public string[] ReadLines()
        {
            return ReadLinesAsync().GetAwaiter().GetResult();
        }

        public async Task<string[]> ReadLinesAsync()
        {
            var result = new List<string>();

            var bytesRead = 0;
            var isBegin = true;

            while (true)
            {
                var line = Reader.ReadLine();

                if (line == null || line == "." || XeDbgResponse.IsStatusMessage(line))
                    break;

                result.Add(line);

                bytesRead += line.Length;

                OnRead(new ClientReadEventArgs(isBegin, bytesRead, 0));

                isBegin = false;
            }

            return [.. result];
        }

        private int ReadDataSize()
        {
            var buffer = new byte[4];

            Client.GetStream().Read(buffer, 0, buffer.Length);

            return BitConverter.ToInt32(buffer);
        }

        public byte[] ReadBytes()
        {
            return ReadBytesAsync().GetAwaiter().GetResult();
        }

        public async Task<byte[]> ReadBytesAsync()
        {
            var bytesRead = 0;
            var bytesTotal = ReadDataSize();

            if (bytesTotal <= 0)
                return [];

            var buffer = new byte[bytesTotal];
            var isBegin = true;

            while (bytesRead < bytesTotal)
            {
                var bufferSize = Math.Min(0x1000, bytesTotal - bytesRead);
                var bufferRead = await Client.GetStream().ReadAsync(buffer, bytesRead, bufferSize);

                if (bufferRead == 0)
                    break;

                bytesRead += bufferRead;

                OnRead(new ClientReadEventArgs(isBegin, bytesRead, bytesTotal));

                isBegin = false;
            }

            return buffer;
        }

        public void CopyTo(Stream in_destination, int in_bufferSize = 81920)
        {
            var bytesRead = 0;
            var bytesTotal = ReadDataSize();

            if (bytesTotal <= 0)
                return;

            // Allocate resizable buffer with preset size.
            var buffer = ArrayPool<byte>.Shared.Rent(in_bufferSize);

            try
            {
                var isBegin = true;

                while (bytesRead < bytesTotal)
                {
                    var bufferSize = Math.Min(0x1000, bytesTotal - bytesRead);
                    var bufferRead = Client.GetStream().Read(buffer, 0, bufferSize);

                    if (bufferRead == 0)
                        break;

                    in_destination.Write(buffer, 0, bufferRead);

                    bytesRead += bufferRead;

                    OnRead(new ClientReadEventArgs(isBegin, bytesRead, bytesTotal));

                    isBegin = false;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        protected virtual void OnRead(ClientReadEventArgs in_args)
        {
            ReadEvent?.Invoke(this, in_args);
        }

        #endregion // Reading Methods

        #region Writing Methods

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
            var isBegin = true;

            while (bytesSent < bytesTotal)
            {
                var bufferSize = Math.Min(0x1000, bytesTotal - bytesSent);
                var buffer = new byte[bufferSize];

                Buffer.BlockCopy(in_data, bytesSent, buffer, 0, bufferSize);

                await Client.GetStream().WriteAsync(buffer);

                bytesSent += bufferSize;

                OnWrite(new ClientWriteEventArgs(isBegin, bytesSent, bytesTotal));

                isBegin = false;
            }
        }

        protected virtual void OnWrite(ClientWriteEventArgs in_args)
        {
            WriteEvent?.Invoke(this, in_args);
        }

        #endregion // Writing Methods

        public void Dispose()
        {
            Client?.Close();
            Reader?.Dispose();
            Writer?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
