using System.Net.Sockets;
using System.Text;

namespace XeSharp.Net.Sockets
{
    public class XeDbgClient : IDisposable
    {
        internal TcpClient Client;
        internal StreamReader Reader;
        internal BinaryWriter Writer;

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

        public string Pop()
        {
            if (!IsConnected())
                return string.Empty;

            return Reader.ReadLine();
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
