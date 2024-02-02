namespace XeSharp.Net.Sockets
{
    public class XeDbgClientInfo
    {
        public bool IsFreebootXBDM { get; private set; }
        public Version DebuggerVersion { get; private set; }

        public XeDbgClientInfo() { }

        public XeDbgClientInfo(XeDbgClient in_client)
        {
            IsFreebootXBDM = in_client.SendCommand("whomadethis").Message.Contains("Natelx");
            DebuggerVersion = new Version(in_client.SendCommand("dmversion").Message ?? "0.0.0.0");
        }
    }
}
