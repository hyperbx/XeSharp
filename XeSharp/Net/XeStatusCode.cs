using XeSharp.Helpers;

namespace XeSharp.Net
{
    public class XeStatusCode
    {
        /// <summary>
        /// The facility pertaining to the HRESULT codes.
        /// </summary>
        public const int Facility = 0x2DA;

        /// <summary>
        /// The raw status code.
        /// </summary>
        public uint Code { get; internal set; }

        public XeStatusCode() { }

        /// <summary>
        /// Creates a status code from a raw code.
        /// </summary>
        /// <param name="in_code">The raw status code.</param>
        public XeStatusCode(uint in_code)
        {
            Code = in_code;
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
        public EXeStatusCode ToHResult()
        {
            return ToHResult(Code);
        }

        /// <summary>
        /// Transforms the input status code into a HRESULT.
        /// </summary>
        /// <param name="in_code">The raw status code to transform.</param>
        /// <returns></returns>
        public static EXeStatusCode ToHResult(uint in_code)
        {
            // TODO: please use maths.
            var str = in_code.ToString();

            return (EXeStatusCode)(((str[0] == '4' ? 1 : 0) << 31) | (Facility << 16) | (int.Parse(str[^1].ToString())));
        }

        /// <summary>
        /// Transforms the input HRESULT into a status code.
        /// </summary>
        /// <param name="in_hResult">The HRESULT to transform.</param>
        public static uint ToStatusCode(EXeStatusCode in_hResult)
        {
            var hResult = (uint)in_hResult;

            if ((hResult >> 16 & 0xFFFF) != Facility)
            {
                // HRESULT is from an unknown facility.
                hResult = unchecked(Win32Helper.IsHResultSuccess((int)hResult)
                    ? (uint)EXeStatusCode.XBDM_NOERR
                    : (uint)EXeStatusCode.XBDM_UNDEFINED);
            }
            else if ((hResult & 0xFFFF) > 0xFF)
            {
                // HRESULT is out of range of the defined values.
                hResult = unchecked((uint)EXeStatusCode.XBDM_UNDEFINED);
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
        public static Dictionary<EXeStatusCode, string> Descriptions { get; } = new()
        {
            { EXeStatusCode.XBDM_NOERR,                             "No error occurred." },
            { EXeStatusCode.XBDM_CONNECTED,                         "A connection has been successfully established." },
            { EXeStatusCode.XBDM_MULTIRESPONSE,                     "One of the three types of continued transactions supported by DmRegisterCommandProcessor." },
            { EXeStatusCode.XBDM_BINRESPONSE,                       "One of the three types of continued transactions supported by DmRegisterCommandProcessor." },
            { EXeStatusCode.XBDM_READYFORBIN,                       "One of the three types of continued transactions supported by DmRegisterCommandProcessor." },
            { EXeStatusCode.XBDM_DEDICATED,                         "A connection has been dedicated to a specific threaded command handler." },
            { EXeStatusCode.XBDM_PROFILERESTARTED,                  "The profiling session has been restarted successfully." },
            { EXeStatusCode.XBDM_FASTCAPENABLED,                    "Fast call-attribute profiling is enabled." },
            { EXeStatusCode.XBDM_CALLCAPENABLED,                    "Calling call-attribute profiling is enabled." },
            { EXeStatusCode.XBDM_RESULTCODE,                        "A result code." },
            { EXeStatusCode.XBDM_UNDEFINED,                         "An undefined error has occurred." },
            { EXeStatusCode.XBDM_MAXCONNECT,                        "The maximum number of connections has been exceeded." },
            { EXeStatusCode.XBDM_NOSUCHFILE,                        "No such file exists." },
            { EXeStatusCode.XBDM_NOMODULE,                          "No such module exists." },
            { EXeStatusCode.XBDM_MEMUNMAPPED,                       "The referenced memory has been unmapped." },
            { EXeStatusCode.XBDM_NOTHREAD,                          "No such thread ID exists." },
            { EXeStatusCode.XBDM_CLOCKNOTSET,                       "The console clock is not set." },
            { EXeStatusCode.XBDM_INVALIDCMD,                        "An invalid command was specified." },
            { EXeStatusCode.XBDM_NOTSTOPPED,                        "Thread not stopped." },
            { EXeStatusCode.XBDM_MUSTCOPY,                          "File must be copied, not moved." },
            { EXeStatusCode.XBDM_ALREADYEXISTS,                     "A file already exists with the same name." },
            { EXeStatusCode.XBDM_DIRNOTEMPTY,                       "The directory is not empty." },
            { EXeStatusCode.XBDM_BADFILENAME,                       "An invalid file name was specified." },
            { EXeStatusCode.XBDM_CANNOTCREATE,                      "Cannot create the specified file." },
            { EXeStatusCode.XBDM_CANNOTACCESS,                      "Cannot access the specified file." },
            { EXeStatusCode.XBDM_DEVICEFULL,                        "The device is full." },
            { EXeStatusCode.XBDM_NOTDEBUGGABLE,                     "This title is not debuggable." },
            { EXeStatusCode.XBDM_BADCOUNTTYPE,                      "The counter type is invalid." },
            { EXeStatusCode.XBDM_COUNTUNAVAILABLE,                  "Counter data is not available." },
            { EXeStatusCode.XBDM_NOTLOCKED,                         "The console is not locked." },
            { EXeStatusCode.XBDM_KEYXCHG,                           "Key exchange is required." },
            { EXeStatusCode.XBDM_MUSTBEDEDICATED,                   "A dedicated connection is required." },
            { EXeStatusCode.XBDM_INVALIDARG,                        "The argument was invalid." },
            { EXeStatusCode.XBDM_PROFILENOTSTARTED,                 "The profile is not started." },
            { EXeStatusCode.XBDM_PROFILEALREADYSTARTED,             "The profile is already started." },
            { EXeStatusCode.XBDM_ALREADYSTOPPED,                    "The console is already in DMN_EXEC_STOP." },
            { EXeStatusCode.XBDM_FASTCAPNOTENABLED,                 "FastCAP is not enabled." },
            { EXeStatusCode.XBDM_NOMEMORY,                          "The Debug Monitor could not allocate memory." },
            { EXeStatusCode.XBDM_TIMEOUT,                           "Initialization through DmStartProfiling has taken longer than allowed." },
            { EXeStatusCode.XBDM_NOSUCHPATH,                        "The path was not found." },
            { EXeStatusCode.XBDM_INVALID_SCREEN_INPUT_FORMAT,       "The screen input format is invalid." },
            { EXeStatusCode.XBDM_INVALID_SCREEN_OUTPUT_FORMAT,      "The screen output format is invalid." },
            { EXeStatusCode.XBDM_CALLCAPNOTENABLED,                 "CallCAP is not enabled." },
            { EXeStatusCode.XBDM_INVALIDCAPCFG,                     "Both FastCAP and CallCAP are enabled in different modules." },
            { EXeStatusCode.XBDM_CAPNOTENABLED,                     "Neither FastCAP nor CallCAP are enabled." },
            { EXeStatusCode.XBDM_TOOBIGJUMP,                        "A branched to a section the instrumentation code failed." },
            { EXeStatusCode.XBDM_FIELDNOTPRESENT,                   "A necessary field is not present in the header of Xbox 360 title." },
            { EXeStatusCode.XBDM_OUTPUTBUFFERTOOSMALL,              "Provided data buffer for profiling is too small." },
            { EXeStatusCode.XBDM_PROFILEREBOOT,                     "The Xbox 360 console is currently rebooting." },
            { EXeStatusCode.XBDM_MAXDURATIONEXCEEDED,               "The maximum duration was exceeded." },
            { EXeStatusCode.XBDM_INVALIDSTATE,                      "The current state of game controller automation is incompatible with the requested action." },
            { EXeStatusCode.XBDM_MAXEXTENSIONS,                     "The maximum number of extensions are already used." },
            { EXeStatusCode.XBDM_PMCSESSIONALREADYACTIVE,           "The Performance Monitor Counters (PMC) session is already active." },
            { EXeStatusCode.XBDM_PMCSESSIONNOTACTIVE,               "The Performance Monitor Counters (PMC) session is not active." },
            { EXeStatusCode.XBDM_LINE_TOO_LONG,                     "The string passed to a debug monitor function, such as DmSendCommand, was too long. The total length of a command string, which includes its null termination and trailing CR/LF must be less than or equal to 512 characters." },
            { EXeStatusCode.XBDM_D3D_DEBUG_COMMAND_NOT_IMPLEMENTED, "The current application has an incompatible version of D3D." },
            { EXeStatusCode.XBDM_D3D_INVALID_SURFACE,               "The D3D surface is not currently valid." },
            { EXeStatusCode.XBDM_CANNOTCONNECT,                     "Cannot connect to the target system." },
            { EXeStatusCode.XBDM_CONNECTIONLOST,                    "The connection to the target system has been lost." },
            { EXeStatusCode.XBDM_FILEERROR,                         "An unexpected file error has occurred." },
            { EXeStatusCode.XBDM_ENDOFLIST,                         "Used by the DmWalkxxx functions to signal the end of a list." },
            { EXeStatusCode.XBDM_BUFFER_TOO_SMALL,                  "The buffer referenced was too small to receive the requested data." },
            { EXeStatusCode.XBDM_NOTXBEFILE,                        "The file specified is not a valid XBE." },
            { EXeStatusCode.XBDM_MEMSETINCOMPLETE,                  "Not all requested memory could be written." },
            { EXeStatusCode.XBDM_NOXBOXNAME,                        "No target system name has been set." },
            { EXeStatusCode.XBDM_NOERRORSTRING,                     "There is no string representation of this error code." },
            { EXeStatusCode.XBDM_INVALIDSTATUS,                     "The Xbox 360 console returns an formatted status string following a command. When using the custom command processor (see DmRegisterCommandProcessor), it may indicate that console and PC code are not compatible." },
            { EXeStatusCode.XBDM_TASK_PENDING,                      "A previous command is still pending." }
        };
    }
}
