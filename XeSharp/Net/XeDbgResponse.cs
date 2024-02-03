using XeSharp.Net.Sockets;

namespace XeSharp.Net
{
    public class XeDbgResponse
    {
        /// <summary>
        /// The status code received by this response.
        /// </summary>
        public XeDbgStatusCode Status { get; private set; } = new XeDbgStatusCode(400);

        /// <summary>
        /// The message received by this response.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The data received by this response.
        /// </summary>
        public object[] Results { get; private set; }

        public XeDbgResponse() { }

        /// <summary>
        /// Creates a response from the client.
        /// </summary>
        /// <param name="in_client">The client to get the response from.</param>
        public XeDbgResponse(XeDbgClient in_client)
        {
            var response = Parse(in_client);

            Status = response.Status;
            Message = response.Message;
            Results = response.Results;
        }

        /// <summary>
        /// Creates a response.
        /// </summary>
        /// <param name="in_status">The status code of the response.</param>
        /// <param name="in_message">The message of the response.</param>
        /// <param name="in_results">The data of the response.</param>
        public XeDbgResponse(XeDbgStatusCode in_status, string in_message, object[] in_results = null)
        {
            Status = in_status;
            Message = in_message;
            Results = in_results;
        }

        /// <summary>
        /// Creates a response.
        /// </summary>
        /// <param name="in_status">The raw status code of the response.</param>
        /// <param name="in_message">The message of the response.</param>
        /// <param name="in_results">The data of the response.</param>
        public XeDbgResponse(uint in_status, string in_message, object[] in_results = null)
            : this(new XeDbgStatusCode(in_status), in_message, in_results) { }

        /// <summary>
        /// Parses the response from the client stream.
        /// </summary>
        /// <param name="in_client">The client to parse the response from.</param>
        /// <param name="in_isAssumeSuccessOnInvalidStatusCode">Determines whether the response will be assumed successful if the status code cannot be parsed from the client stream.</param>
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

        /// <summary>
        /// Parses the response from the client stream.
        /// </summary>
        /// <param name="in_client">The client to parse the response from.</param>
        /// <param name="out_response">The response parsed from the client stream.</param>
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

        /// <summary>
        /// Determines whether the input string pertains to a status message.
        /// </summary>
        /// <param name="in_str">The string to check.</param>
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
