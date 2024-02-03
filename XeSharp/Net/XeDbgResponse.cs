using XeSharp.Net.Sockets;

namespace XeSharp.Net
{
    public class XeDbgResponse
    {
        public XeDbgStatusCode Status { get; private set; } = new XeDbgStatusCode(400);
        public string Message { get; private set; }
        public object[] Results { get; private set; }

        public XeDbgResponse() { }

        public XeDbgResponse(XeDbgClient in_client)
        {
            var response = Parse(in_client);

            Status = response.Status;
            Message = response.Message;
            Results = response.Results;
        }

        public XeDbgResponse(XeDbgStatusCode in_status, string in_message, object[] in_results = null)
        {
            Status = in_status;
            Message = in_message;
            Results = in_results;
        }

        public XeDbgResponse(uint in_status, string in_message, object[] in_results = null) : this(new XeDbgStatusCode(in_status), in_message, in_results) { }

        public static XeDbgResponse Parse(XeDbgClient in_client, bool in_isAssumeSuccessOnInvalidStatusCode = false)
        {
            var buffer = in_client.Reader.ReadLine();

            if (string.IsNullOrEmpty(buffer))
                return new XeDbgResponse();

            var tokens = buffer.Split('-', StringSplitOptions.RemoveEmptyEntries);

            var status = 400U;
            var isStatusParsed = true;

            if (!uint.TryParse(tokens[0], out status))
            {
                // HACK: necessity for custom commands in Natelx's version of XBDM.
                if (in_isAssumeSuccessOnInvalidStatusCode || in_client.Info?.IsFreebootXBDM == true)
                {
                    status = XeDbgStatusCode.ToStatusCode(EXeDbgStatusCode.XBDM_NOERR);
                    isStatusParsed = false;
                }
                else
                {
                    throw new FormatException($"Failed to parse status code from response: {tokens[0]}");
                }
            }

            var hResult = XeDbgStatusCode.ToHResult(status);
            var message = tokens[isStatusParsed ? 1 : 0].Trim();

            /* Handle binary response manually post-response.
               We could read the data here straight into a buffer,
               but we may run out of memory if not streamed somewhere. */
            if (hResult == EXeDbgStatusCode.XBDM_BINRESPONSE)
                return new XeDbgResponse(status, message);

            var isMultiResponse = hResult == EXeDbgStatusCode.XBDM_MULTIRESPONSE;

            if (!string.IsNullOrEmpty(message))
            {
                // HACK: necessity for custom "hwinfo" command in Natelx's version of XBDM.
                if (in_client.Info?.IsFreebootXBDM == true && !isMultiResponse && message.ToLower().EndsWith("follows"))
                    isMultiResponse = true;
            }

            // Handle multi-line response.
            if (isMultiResponse || !isStatusParsed)
                return new XeDbgResponse(status, message, in_client.ReadLines());

            return new XeDbgResponse(status, message);
        }

        public static bool TryParse(XeDbgClient in_client, out XeDbgResponse out_response)
        {
            try
            {
                out_response = Parse(in_client, false);
                return true;
            }
            catch
            {
                out_response = null;
                return false;
            }
        }

        public static bool IsStatusMessage(string in_str)
        {
            if (string.IsNullOrEmpty(in_str) || in_str.Length <= 0 || in_str.Length < 3)
                return false;

            bool isStatusCode = char.IsDigit(in_str[0]) && char.IsDigit(in_str[1]) && char.IsDigit(in_str[2]);

            if (in_str.Length >= 5)
                return isStatusCode && in_str[3] == '-' && in_str[4] == ' ';

            return isStatusCode;
        }
    }
}
