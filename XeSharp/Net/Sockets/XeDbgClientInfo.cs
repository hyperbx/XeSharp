namespace XeSharp.Net.Sockets
{
    public class XeDbgClientInfo
    {
        /// <summary>
        /// Determines whether the XBDM module on the server is Natelx's custom Freeboot version.
        /// </summary>
        public bool IsFreebootXBDM { get; private set; }

        /// <summary>
        /// The version of XBDM on the server.
        /// <para>This is always 2.0.20353.0 for Freeboot XBDM.</para>
        /// </summary>
        public Version DebuggerVersion { get; private set; }

        public XeDbgClientInfo() { }

        public XeDbgClientInfo(XeDbgClient in_client)
        {
            IsFreebootXBDM = in_client.SendCommand("whomadethis").Message.Contains("Natelx");
            DebuggerVersion = new Version(in_client.SendCommand("dmversion").Message ?? "0.0.0.0");
        }
    }
}
