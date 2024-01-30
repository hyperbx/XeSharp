namespace XeSharp.Net
{
    // C# users yearn for macros, at least, I do.
    public enum EXeDbgStatusCode
    {
        /// <summary>
        /// No error occurred.
        /// </summary>
        XBDM_NOERR = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0),

        /// <summary>
        /// A connection has been successfully established.
        /// </summary>
        XBDM_CONNECTED = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 1),

        /// <summary>
        /// One of the three types of continued transactions supported by DmRegisterCommandProcessor.
        /// </summary>
        XBDM_MULTIRESPONSE = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 2),

        /// <summary>
        /// One of the three types of continued transactions supported by DmRegisterCommandProcessor.
        /// </summary>
        XBDM_BINRESPONSE = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 3),

        /// <summary>
        /// One of the three types of continued transactions supported by DmRegisterCommandProcessor.
        /// </summary>
        XBDM_READYFORBIN = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 4),

        /// <summary>
        /// A connection has been dedicated to a specific threaded command handler.
        /// </summary>
        XBDM_DEDICATED = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 5),

        /// <summary>
        /// The profiling session has been restarted successfully.
        /// </summary>
        XBDM_PROFILERESTARTED = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 6),

        /// <summary>
        /// Fast call-attribute profiling is enabled.
        /// </summary>
        XBDM_FASTCAPENABLED = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 7),

        /// <summary>
        /// Calling call-attribute profiling is enabled.
        /// </summary>
        XBDM_CALLCAPENABLED = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 8),

        /// <summary>
        /// A result code.
        /// </summary>
        XBDM_RESULTCODE = ((0 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 9),

        /// <summary>
        /// An undefined error has occurred.
        /// </summary>
        XBDM_UNDEFINED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0),

        /// <summary>
        /// The maximum number of connections has been exceeded.
        /// </summary>
        XBDM_MAXCONNECT = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 1),

        /// <summary>
        /// No such file exists.
        /// </summary>
        XBDM_NOSUCHFILE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 2),

        /// <summary>
        /// No such module exists.
        /// </summary>
        XBDM_NOMODULE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 3),

        /// <summary>
        /// The referenced memory has been unmapped.
        /// </summary>
        XBDM_MEMUNMAPPED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 4),

        /// <summary>
        /// No such thread ID exists.
        /// </summary>
        XBDM_NOTHREAD = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 5),

        /// <summary>
        /// The console clock is not set.
        /// </summary>
        XBDM_CLOCKNOTSET = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 6),

        /// <summary>
        /// An invalid command was specified.
        /// </summary>
        XBDM_INVALIDCMD = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 7),

        /// <summary>
        /// Thread not stopped.
        /// </summary>
        XBDM_NOTSTOPPED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 8),

        /// <summary>
        /// File must be copied, not moved.
        /// </summary>
        XBDM_MUSTCOPY = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 9),

        /// <summary>
        /// A file already exists with the same name.
        /// </summary>
        XBDM_ALREADYEXISTS = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 10),

        /// <summary>
        /// The directory is not empty.
        /// </summary>
        XBDM_DIRNOTEMPTY = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 11),

        /// <summary>
        /// An invalid file name was specified.
        /// </summary>
        XBDM_BADFILENAME = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 12),

        /// <summary>
        /// Cannot create the specified file.
        /// </summary>
        XBDM_CANNOTCREATE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 13),

        /// <summary>
        /// Cannot access the specified file.
        /// </summary>
        XBDM_CANNOTACCESS = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 14),

        /// <summary>
        /// The device is full.
        /// </summary>
        XBDM_DEVICEFULL = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 15),

        /// <summary>
        /// This title is not debuggable.
        /// </summary>
        XBDM_NOTDEBUGGABLE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 16),

        /// <summary>
        /// The counter type is invalid.
        /// </summary>
        XBDM_BADCOUNTTYPE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 17),

        /// <summary>
        /// Counter data is not available.
        /// </summary>
        XBDM_COUNTUNAVAILABLE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 18),

        /// <summary>
        /// The console is not locked.
        /// </summary>
        XBDM_NOTLOCKED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 20),

        /// <summary>
        /// Key exchange is required.
        /// </summary>
        XBDM_KEYXCHG = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 21),

        /// <summary>
        /// A dedicated connection is required.
        /// </summary>
        XBDM_MUSTBEDEDICATED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 22),

        /// <summary>
        /// The argument was invalid.
        /// </summary>
        XBDM_INVALIDARG = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 23),

        /// <summary>
        /// The profile is not started.
        /// </summary>
        XBDM_PROFILENOTSTARTED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 24),

        /// <summary>
        /// The profile is already started.
        /// </summary>
        XBDM_PROFILEALREADYSTARTED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 25),

        /// <summary>
        /// The console is already in DMN_EXEC_STOP.
        /// </summary>
        XBDM_ALREADYSTOPPED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 26),

        /// <summary>
        /// FastCAP is not enabled.
        /// </summary>
        XBDM_FASTCAPNOTENABLED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 27),

        /// <summary>
        /// The Debug Monitor could not allocate memory.
        /// </summary>
        XBDM_NOMEMORY = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 28),

        /// <summary>
        /// Initialization through DmStartProfiling has taken longer than allowed.
        /// </summary>
        XBDM_TIMEOUT = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 29),

        /// <summary>
        /// The path was not found.
        /// </summary>
        XBDM_NOSUCHPATH = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 30),

        /// <summary>
        /// The screen input format is invalid.
        /// </summary>
        XBDM_INVALID_SCREEN_INPUT_FORMAT = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 31),

        /// <summary>
        /// The screen output format is invalid.
        /// </summary>
        XBDM_INVALID_SCREEN_OUTPUT_FORMAT = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 32),

        /// <summary>
        /// CallCAP is not enabled.
        /// </summary>
        XBDM_CALLCAPNOTENABLED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 33),

        /// <summary>
        /// Both FastCAP and CallCAP are enabled in different modules.
        /// </summary>
        XBDM_INVALIDCAPCFG = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 34),

        /// <summary>
        /// Neither FastCAP nor CallCAP are enabled.
        /// </summary>
        XBDM_CAPNOTENABLED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 35),

        /// <summary>
        /// A branched to a section the instrumentation code failed.
        /// </summary>
        XBDM_TOOBIGJUMP = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 36),

        /// <summary>
        /// A necessary field is not present in the header of Xbox 360 title.
        /// </summary>
        XBDM_FIELDNOTPRESENT = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 37),

        /// <summary>
        /// Provided data buffer for profiling is too small.
        /// </summary>
        XBDM_OUTPUTBUFFERTOOSMALL = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 38),

        /// <summary>
        /// The Xbox 360 console is currently rebooting.
        /// </summary>
        XBDM_PROFILEREBOOT = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 39),

        /// <summary>
        /// The maximum duration was exceeded.
        /// </summary>
        XBDM_MAXDURATIONEXCEEDED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 41),

        /// <summary>
        /// The current state of game controller automation is incompatible with the requested action.
        /// </summary>
        XBDM_INVALIDSTATE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 42),

        /// <summary>
        /// The maximum number of extensions are already used.
        /// </summary>
        XBDM_MAXEXTENSIONS = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 43),

        /// <summary>
        /// The Performance Monitor Counters (PMC) session is already active.
        /// </summary>
        XBDM_PMCSESSIONALREADYACTIVE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 44),

        /// <summary>
        /// The Performance Monitor Counters (PMC) session is not active.
        /// </summary>
        XBDM_PMCSESSIONNOTACTIVE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 45),

        /// <summary>
        /// The string passed to a debug monitor function, such as DmSendCommand, was too long.
        /// The total length of a command string, which includes its null termination and trailing CR/LF must be less than or equal to 512 characters.
        /// </summary>
        XBDM_LINE_TOO_LONG = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 46),

        /// <summary>
        /// The current application has an incompatible version of D3D.
        /// </summary>
        XBDM_D3D_DEBUG_COMMAND_NOT_IMPLEMENTED = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x50),

        /// <summary>
        /// The D3D surface is not currently valid.
        /// </summary>
        XBDM_D3D_INVALID_SURFACE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x51),

        /// <summary>
        /// Cannot connect to the target system.
        /// </summary>
        XBDM_CANNOTCONNECT = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x100),

        /// <summary>
        /// The connection to the target system has been lost.
        /// </summary>
        XBDM_CONNECTIONLOST = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x101),

        /// <summary>
        /// An unexpected file error has occurred.
        /// </summary>
        XBDM_FILEERROR = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x103),

        /// <summary>
        /// Used by the DmWalkxxx functions to signal the end of a list.
        /// </summary>
        XBDM_ENDOFLIST = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x104),

        /// <summary>
        /// The buffer referenced was too small to receive the requested data.
        /// </summary>
        XBDM_BUFFER_TOO_SMALL = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x105),

        /// <summary>
        /// The file specified is not a valid XBE.
        /// </summary>
        XBDM_NOTXBEFILE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x106),

        /// <summary>
        /// Not all requested memory could be written.
        /// </summary>
        XBDM_MEMSETINCOMPLETE = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x107),

        /// <summary>
        /// No target system name has been set.
        /// </summary>
        XBDM_NOXBOXNAME = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x108),

        /// <summary>
        /// There is no string representation of this error code.
        /// </summary>
        XBDM_NOERRORSTRING = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x109),

        /// <summary>
        /// The Xbox 360 console returns an formatted status string following a command.
        /// When using the custom command processor (see DmRegisterCommandProcessor), it may indicate that console and PC code are not compatible.
        /// </summary>
        XBDM_INVALIDSTATUS = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x10A),

        /// <summary>
        /// A previous command is still pending.
        /// </summary>
        XBDM_TASK_PENDING = ((1 << 31) | (XeDbgStatusCode.FACILITY_XBDM << 16) | 0x150)
    }
}
