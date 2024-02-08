using XeSharp.Helpers;

namespace XeSharp.Net
{
    public class XeDbgStatusCode
    {
        /// <summary>
        /// The facility pertaining to the HRESULT codes.
        /// </summary>
        public const int Facility = 0x2DA;

        /// <summary>
        /// The raw status code.
        /// </summary>
        public uint StatusCode { get; internal set; }

        public XeDbgStatusCode() { }

        /// <summary>
        /// Creates a status code from a raw code.
        /// </summary>
        /// <param name="in_statusCode">The raw status code.</param>
        public XeDbgStatusCode(uint in_statusCode)
        {
            StatusCode = in_statusCode;
        }

        /// <summary>
        /// Determines whether the HRESULT of this status code is successful.
        /// </summary>
        public bool IsSucceeded()
        {
            return Win32Helper.IsHResultSuccess((int)ToHResult());
        }

        /// <summary>
        /// Determines whether the HRESULT of this status code has failed.
        /// </summary>
        public bool IsFailed()
        {
            return Win32Helper.IsHResultFail((int)ToHResult());
        }

        /// <summary>
        /// Transforms this status code into a HRESULT.
        /// </summary>
        public EXeDbgStatusCode ToHResult()
        {
            return ToHResult(StatusCode);
        }

        /// <summary>
        /// Transforms the input status code into a HRESULT.
        /// </summary>
        /// <param name="in_statusCode">The raw status code to transform.</param>
        /// <returns></returns>
        public static EXeDbgStatusCode ToHResult(uint in_statusCode)
        {
            // TODO: please use maths.
            var str = in_statusCode.ToString();

            return (EXeDbgStatusCode)(((str[0] == '4' ? 1 : 0) << 31) | (Facility << 16) | (int.Parse(str[^1].ToString())));
        }

        /// <summary>
        /// Transforms the input HRESULT into a status code.
        /// </summary>
        /// <param name="in_hResult">The HRESULT to transform.</param>
        public static uint ToStatusCode(EXeDbgStatusCode in_hResult)
        {
            var hResult = (uint)in_hResult;

            if ((hResult >> 16 & 0xFFFF) != Facility)
            {
                // HRESULT is from an unknown facility.
                hResult = unchecked(Win32Helper.IsHResultSuccess((int)hResult)
                    ? (uint)EXeDbgStatusCode.XBDM_NOERR
                    : (uint)EXeDbgStatusCode.XBDM_UNDEFINED);
            }
            else if ((hResult & 0xFFFF) > 0xFF)
            {
                // HRESULT is out of range of the defined values.
                hResult = unchecked((uint)EXeDbgStatusCode.XBDM_UNDEFINED);
            }

            string code = Win32Helper.IsHResultFail((int)hResult >> 31) ? "4" : "2";
            code += (char)('0' + ((hResult & 0xFFFF) / 10));
            code += (char)('0' + ((hResult & 0xFFFF) % 10));

            return uint.Parse(code);
        }

        public override string ToString()
        {
            var hResult = ToHResult();

            return $"HRESULT 0x{((uint)hResult):X8} ({hResult}): {Descriptions[hResult]}";
        }

        /// <summary>
        /// The descriptions pertaining to each HRESULT.
        /// </summary>
        public static Dictionary<EXeDbgStatusCode, string> Descriptions { get; } = new()
        {
            { EXeDbgStatusCode.XBDM_NOERR,                             "No error occurred." },
            { EXeDbgStatusCode.XBDM_CONNECTED,                         "A connection has been successfully established." },
            { EXeDbgStatusCode.XBDM_MULTIRESPONSE,                     "One of the three types of continued transactions supported by DmRegisterCommandProcessor." },
            { EXeDbgStatusCode.XBDM_BINRESPONSE,                       "One of the three types of continued transactions supported by DmRegisterCommandProcessor." },
            { EXeDbgStatusCode.XBDM_READYFORBIN,                       "One of the three types of continued transactions supported by DmRegisterCommandProcessor." },
            { EXeDbgStatusCode.XBDM_DEDICATED,                         "A connection has been dedicated to a specific threaded command handler." },
            { EXeDbgStatusCode.XBDM_PROFILERESTARTED,                  "The profiling session has been restarted successfully." },
            { EXeDbgStatusCode.XBDM_FASTCAPENABLED,                    "Fast call-attribute profiling is enabled." },
            { EXeDbgStatusCode.XBDM_CALLCAPENABLED,                    "Calling call-attribute profiling is enabled." },
            { EXeDbgStatusCode.XBDM_RESULTCODE,                        "A result code." },
            { EXeDbgStatusCode.XBDM_UNDEFINED,                         "An undefined error has occurred." },
            { EXeDbgStatusCode.XBDM_MAXCONNECT,                        "The maximum number of connections has been exceeded." },
            { EXeDbgStatusCode.XBDM_NOSUCHFILE,                        "No such file exists." },
            { EXeDbgStatusCode.XBDM_NOMODULE,                          "No such module exists." },
            { EXeDbgStatusCode.XBDM_MEMUNMAPPED,                       "The referenced memory has been unmapped." },
            { EXeDbgStatusCode.XBDM_NOTHREAD,                          "No such thread ID exists." },
            { EXeDbgStatusCode.XBDM_CLOCKNOTSET,                       "The console clock is not set." },
            { EXeDbgStatusCode.XBDM_INVALIDCMD,                        "An invalid command was specified." },
            { EXeDbgStatusCode.XBDM_NOTSTOPPED,                        "Thread not stopped." },
            { EXeDbgStatusCode.XBDM_MUSTCOPY,                          "File must be copied, not moved." },
            { EXeDbgStatusCode.XBDM_ALREADYEXISTS,                     "A file already exists with the same name." },
            { EXeDbgStatusCode.XBDM_DIRNOTEMPTY,                       "The directory is not empty." },
            { EXeDbgStatusCode.XBDM_BADFILENAME,                       "An invalid file name was specified." },
            { EXeDbgStatusCode.XBDM_CANNOTCREATE,                      "Cannot create the specified file." },
            { EXeDbgStatusCode.XBDM_CANNOTACCESS,                      "Cannot access the specified file." },
            { EXeDbgStatusCode.XBDM_DEVICEFULL,                        "The device is full." },
            { EXeDbgStatusCode.XBDM_NOTDEBUGGABLE,                     "This title is not debuggable." },
            { EXeDbgStatusCode.XBDM_BADCOUNTTYPE,                      "The counter type is invalid." },
            { EXeDbgStatusCode.XBDM_COUNTUNAVAILABLE,                  "Counter data is not available." },
            { EXeDbgStatusCode.XBDM_NOTLOCKED,                         "The console is not locked." },
            { EXeDbgStatusCode.XBDM_KEYXCHG,                           "Key exchange is required." },
            { EXeDbgStatusCode.XBDM_MUSTBEDEDICATED,                   "A dedicated connection is required." },
            { EXeDbgStatusCode.XBDM_INVALIDARG,                        "The argument was invalid." },
            { EXeDbgStatusCode.XBDM_PROFILENOTSTARTED,                 "The profile is not started." },
            { EXeDbgStatusCode.XBDM_PROFILEALREADYSTARTED,             "The profile is already started." },
            { EXeDbgStatusCode.XBDM_ALREADYSTOPPED,                    "The console is already in DMN_EXEC_STOP." },
            { EXeDbgStatusCode.XBDM_FASTCAPNOTENABLED,                 "FastCAP is not enabled." },
            { EXeDbgStatusCode.XBDM_NOMEMORY,                          "The Debug Monitor could not allocate memory." },
            { EXeDbgStatusCode.XBDM_TIMEOUT,                           "Initialization through DmStartProfiling has taken longer than allowed." },
            { EXeDbgStatusCode.XBDM_NOSUCHPATH,                        "The path was not found." },
            { EXeDbgStatusCode.XBDM_INVALID_SCREEN_INPUT_FORMAT,       "The screen input format is invalid." },
            { EXeDbgStatusCode.XBDM_INVALID_SCREEN_OUTPUT_FORMAT,      "The screen output format is invalid." },
            { EXeDbgStatusCode.XBDM_CALLCAPNOTENABLED,                 "CallCAP is not enabled." },
            { EXeDbgStatusCode.XBDM_INVALIDCAPCFG,                     "Both FastCAP and CallCAP are enabled in different modules." },
            { EXeDbgStatusCode.XBDM_CAPNOTENABLED,                     "Neither FastCAP nor CallCAP are enabled." },
            { EXeDbgStatusCode.XBDM_TOOBIGJUMP,                        "A branched to a section the instrumentation code failed." },
            { EXeDbgStatusCode.XBDM_FIELDNOTPRESENT,                   "A necessary field is not present in the header of Xbox 360 title." },
            { EXeDbgStatusCode.XBDM_OUTPUTBUFFERTOOSMALL,              "Provided data buffer for profiling is too small." },
            { EXeDbgStatusCode.XBDM_PROFILEREBOOT,                     "The Xbox 360 console is currently rebooting." },
            { EXeDbgStatusCode.XBDM_MAXDURATIONEXCEEDED,               "The maximum duration was exceeded." },
            { EXeDbgStatusCode.XBDM_INVALIDSTATE,                      "The current state of game controller automation is incompatible with the requested action." },
            { EXeDbgStatusCode.XBDM_MAXEXTENSIONS,                     "The maximum number of extensions are already used." },
            { EXeDbgStatusCode.XBDM_PMCSESSIONALREADYACTIVE,           "The Performance Monitor Counters (PMC) session is already active." },
            { EXeDbgStatusCode.XBDM_PMCSESSIONNOTACTIVE,               "The Performance Monitor Counters (PMC) session is not active." },
            { EXeDbgStatusCode.XBDM_LINE_TOO_LONG,                     "The string passed to a debug monitor function, such as DmSendCommand, was too long. The total length of a command string, which includes its null termination and trailing CR/LF must be less than or equal to 512 characters." },
            { EXeDbgStatusCode.XBDM_D3D_DEBUG_COMMAND_NOT_IMPLEMENTED, "The current application has an incompatible version of D3D." },
            { EXeDbgStatusCode.XBDM_D3D_INVALID_SURFACE,               "The D3D surface is not currently valid." },
            { EXeDbgStatusCode.XBDM_CANNOTCONNECT,                     "Cannot connect to the target system." },
            { EXeDbgStatusCode.XBDM_CONNECTIONLOST,                    "The connection to the target system has been lost." },
            { EXeDbgStatusCode.XBDM_FILEERROR,                         "An unexpected file error has occurred." },
            { EXeDbgStatusCode.XBDM_ENDOFLIST,                         "Used by the DmWalkxxx functions to signal the end of a list." },
            { EXeDbgStatusCode.XBDM_BUFFER_TOO_SMALL,                  "The buffer referenced was too small to receive the requested data." },
            { EXeDbgStatusCode.XBDM_NOTXBEFILE,                        "The file specified is not a valid XBE." },
            { EXeDbgStatusCode.XBDM_MEMSETINCOMPLETE,                  "Not all requested memory could be written." },
            { EXeDbgStatusCode.XBDM_NOXBOXNAME,                        "No target system name has been set." },
            { EXeDbgStatusCode.XBDM_NOERRORSTRING,                     "There is no string representation of this error code." },
            { EXeDbgStatusCode.XBDM_INVALIDSTATUS,                     "The Xbox 360 console returns an formatted status string following a command. When using the custom command processor (see DmRegisterCommandProcessor), it may indicate that console and PC code are not compatible." },
            { EXeDbgStatusCode.XBDM_TASK_PENDING,                      "A previous command is still pending." }
        };
    }
}
