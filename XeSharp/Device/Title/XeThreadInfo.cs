using System.Diagnostics.CodeAnalysis;
using XeSharp.Exceptions;
using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Serialisation.INI;

namespace XeSharp.Device.Title
{
    public class XeThreadInfo
    {
        /// <summary>
        /// The ID of this thread.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Determines whether this thread has been suspended.
        /// </summary>
        [IniProperty(Key = "suspend")] public bool IsSuspended { get; set; }

        /// <summary>
        /// The priority of this thread.
        /// </summary>
        [IniProperty(Key = "priority")] public int Priority { get; set; }

        /// <summary>
        /// The base address for thread local storage.
        /// </summary>
        [IniProperty(Key = "tlsbase")] public uint TLSBase { get; set; }

        /// <summary>
        /// The address this thread starts in memory.
        /// </summary>
        [IniProperty(Key = "start")] public uint StartAddress { get; set; }

        /// <summary>
        /// The base address for the stack pertaining to this thread.
        /// </summary>
        [IniProperty(Key = "base")] public uint StackBase { get; set; }

        /// <summary>
        /// The limit address for the stack.
        /// </summary>
        [IniProperty(Key = "limit")] public uint StackLimit { get; set; }

        /// <summary>
        /// The unused space in the stack.
        /// </summary>
        [IniProperty(Key = "slack")] public uint StackSlackSpace { get; set; }

        /// <summary>
        /// The date and time this thread was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// The address to the name of this thread.
        /// </summary>
        [IniProperty(Key = "nameaddr")] public uint NameAddress { get; set; }

        /// <summary>
        /// The length of the name of this thread.
        /// </summary>
        [IniProperty(Key = "namelen")] public uint NameLength { get; set; }

        /// <summary>
        /// The index of the logical processor running this thread.
        /// </summary>
        [IniProperty(Key = "proc")] public byte ProcessorIndex { get; set; }

        /// <summary>
        /// The last error code from this thread.
        /// </summary>
        [IniProperty(Key = "lasterr")] public uint LastError { get; set; }

        /// <summary>
        /// Creates a new thread by ID.
        /// </summary>
        /// <param name="in_console">The console the thread is running on.</param>
        /// <param name="in_threadID">The ID of the thread.</param>
        public XeThreadInfo(XeConsole in_console, int in_threadID)
        {
            var response = in_console.Client.SendCommand($"threadinfo thread=0x{in_threadID:X}");

            if (response.Status.ToHResult() == EXeStatusCode.XBDM_NOTHREAD)
                throw new ThreadNotFoundException(in_threadID);

            if (response.Status.ToHResult() != EXeStatusCode.XBDM_MULTIRESPONSE)
                throw new InvalidDataException("Failed to obtain thread information.");

            Parse(response.Results[0] as string);

            ID = in_threadID;
        }

        private void Parse(string in_ini)
        {
            var ini = IniParser.DoInline(this, in_ini);

            DateCreated = FormatHelper.FromFileTime(
                MemoryHelper.ChangeType<uint>(ini[""]["createhi"]),
                MemoryHelper.ChangeType<uint>(ini[""]["createlo"]));
        }

        public override bool Equals([NotNullWhen(true)] object? in_obj)
        {
            if (in_obj is XeThreadInfo threadInfo)
            {
                return IsSuspended == threadInfo.IsSuspended &&
                       Priority == threadInfo.Priority &&
                       TLSBase == threadInfo.TLSBase &&
                       StartAddress == threadInfo.StartAddress &&
                       StackBase == threadInfo.StackBase &&
                       StackLimit == threadInfo.StackLimit &&
                       StackSlackSpace == threadInfo.StackSlackSpace &&
                       DateCreated == threadInfo.DateCreated &&
                       NameAddress == threadInfo.NameAddress &&
                       NameLength == threadInfo.NameLength &&
                       ProcessorIndex == threadInfo.ProcessorIndex &&
                       LastError == threadInfo.LastError;
            }

            return false;
        }
    }
}
