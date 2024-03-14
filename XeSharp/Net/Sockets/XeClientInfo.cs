namespace XeSharp.Net.Sockets
{
    public class XeClientInfo
    {
        /// <summary>
        /// Determines whether the XBDM module on the server is Natelx's custom Freeboot version.
        /// </summary>
        public bool IsFreebootXBDM { get; private set; }

        /// <summary>
        /// The XDK version of XBDM on the server.
        /// </summary>
        public Version DebuggerVersion { get; private set; }

        public XeClientInfo() { }

        public XeClientInfo(XeClient in_client)
        {
            var author = in_client.SendCommand("whomadethis", false);

            if (author.Status.ToHResult() == EXeStatusCode.XBDM_NOERR)
                IsFreebootXBDM = author.Message.Contains("Natelx");

            DebuggerVersion = new Version(in_client.SendCommand("dmversion").Message ?? "0.0.0.0");
        }
    }
}
