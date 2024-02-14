using XeSharp.Debug.Processor;
using XeSharp.Device;
using XeSharp.Device.Title;
using XeSharp.Helpers;
using XeSharp.Net;
using XeSharp.Serialisation.INI;

namespace XeSharp.Debug
{
    public class XeDebugger : IDisposable
    {
        protected XeConsole _console;

        public XeDebugger() { }

        /// <summary>
        /// Creates a new debugger.
        /// </summary>
        /// <param name="in_console">The console to attach the debugger to.</param>
        /// <param name="in_isAttachOnLoad">Determines whether this debugger will attach to the console upon creation.</param>
        public XeDebugger(XeConsole in_console, bool in_isAttachOnLoad = true)
        {
            _console = in_console;

            if (!in_isAttachOnLoad)
                return;

            Attach();
        }

        /// <summary>
        /// Creates a new debugger.
        /// </summary>
        /// <param name="in_hostName">The host name or IP address of the console to attach the debugger to.</param>
        /// <param name="in_isAttachOnLoad">Determines whether this debugger will attach to the console upon creation.</param>
        public XeDebugger(string in_hostName, bool in_isAttachOnLoad = true)
            : this(new XeConsole(in_hostName, true, false), in_isAttachOnLoad) { }

        /// <summary>
        /// Attaches the debugger to the input console.
        /// </summary>
        /// <param name="in_console">The console to attach to.</param>
        public static void Attach(XeConsole in_console)
        {
            in_console.Client.SendCommand($"debugger override connect name=\"XeSharp\" user=\"{Environment.UserName}\"", false);
        }

        /// <summary>
        /// Detaches the debugger from the input console.
        /// </summary>
        /// <param name="in_console">The console to detach from.</param>
        public static void Detach(XeConsole in_console)
        {
            in_console.Client.SendCommand("debugger override disconnect", false);
        }

        /// <summary>
        /// Attaches the debugger to the console.
        /// </summary>
        public void Attach()
        {
            Attach(_console);
        }

        /// <summary>
        /// Detaches the debugger from the console.
        /// </summary>
        public void Detach()
        {
            Detach(_console);
        }

        /// <summary>
        /// Determines whether the debugger is attached to the console.
        /// </summary>
        public bool IsAttached()
        {
            return _console.Client.SendCommand("isdebugger", false).Status.Code == 410;
        }

        /// <summary>
        /// Places a breakpoint at the specified address.
        /// </summary>
        /// <param name="in_addr">The virtual address to break on.</param>
        public void AddBreakpoint(uint in_addr)
        {
            if (!IsAttached())
                Attach();
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Breakpoint added at 0x{in_addr:X8}");
#endif
            _console.Client.SendCommand($"break addr=0x{in_addr:X8}", false);
        }

        /// <summary>
        /// Removes a breakpoint from the specified address.
        /// </summary>
        /// <param name="in_addr">The virtual address to stop breaking on.</param>
        public void RemoveBreakpoint(uint in_addr)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Breakpoint removed at 0x{in_addr:X8}");
#endif

            _console.Client.SendCommand($"break clear addr=0x{in_addr:X8}", false);
        }

        /// <summary>
        /// Gets the current breakpoint address the remote console is stopped on.
        /// </summary>
        /// <param name="in_threadID">The ID of the thread that is stopped.</param>
        public uint GetBreakpoint(int in_threadID = XeConsole.MainThreadID)
        {
            var response = _console.Client.SendCommand($"isstopped thread=0x{in_threadID:X8}", false);

            if (response.Status.ToHResult() != EXeStatusCode.XBDM_NOERR)
                return 0;

            var ini = IniParser.DoInline(response.Message);

            if (ini[""].TryGetValue("addr", out string out_addr))
                return MemoryHelper.ChangeType<uint>(out_addr);

            return 0;
        }

        /// <summary>
        /// Clears all breakpoints.
        /// </summary>
        public void ClearBreakpoints()
        {
            _console.Client.SendCommand("break clearall", false);
        }

        /// <summary>
        /// Resumes execution.
        /// </summary>
        public void Go()
        {
            _console.Client.SendCommand("go", false);
        }

        /// <summary>
        /// Determines whether the specified thread is stopped.
        /// </summary>
        /// <param name="in_threadID">The thread to check.</param>
        public bool IsStopped(int in_threadID = XeConsole.MainThreadID)
        {
            var response = _console.Client.SendCommand($"isstopped thread=0x{in_threadID:X8}", false);

            return response.Status.ToHResult() == EXeStatusCode.XBDM_NOERR;
        }

        /// <summary>
        /// Steps into the next instruction.
        /// </summary>
        /// <param name="in_threadID">The thread to step through.</param>
        /// <param name="in_amount">The amount of instructions to step into.</param>
        public XeProcessor StepInto(int in_threadID = XeConsole.MainThreadID, int in_amount = 1)
        {
            var processor = GetProcessor(in_threadID);

            if (!IsStopped(in_threadID))
                return processor;

            var stepTo = processor.IAR;

            for (int i = 0; i < in_amount; i++)
                stepTo += 4;

            // TODO: walk branches.
            RemoveBreakpoint(processor.IAR);
            AddBreakpoint(stepTo);
            Go();

            return GetProcessor(in_threadID);
        }

        /// <summary>
        /// Steps over the next instruction.
        /// </summary>
        /// <param name="in_threadID">The thread to step through.</param>
        public XeProcessor StepOver(int in_threadID = XeConsole.MainThreadID)
        {
            return StepInto(in_threadID, 2);
        }

        /// <summary>
        /// Gets the thread stopped by the specified breakpoint address.
        /// </summary>
        /// <param name="in_addr">The address of a breakpoint stopped on to locate its thread.</param>
        /// <returns></returns>
        public XeThreadInfo GetThreadFromBreakpoint(uint in_addr)
        {
            foreach (var thread in _console.GetThreads())
            {
                var processor = GetProcessor(thread.Key);

                if (processor.IAR == in_addr)
                    return thread.Value;
            }

            return null;
        }

        /// <summary>
        /// Gets the processor state of the specified thread.
        /// </summary>
        /// <param name="in_threadID">The thread to retrieve CPU information from.</param>
        /// <param name="in_isCommitOnChange">Determines whether registers will be committed to the remote console upon changing them.</param>
        public XeProcessor GetProcessor(int in_threadID = XeConsole.MainThreadID, bool in_isCommitOnChange = true)
        {
            return new XeProcessor(_console, new XeThreadInfo(_console, in_threadID), in_isCommitOnChange);
        }

        /// <summary>
        /// Gets the processor state of the specified thread.
        /// <para>This type will commit all changes to the remote console upon disposing it manually or via a "using" clause.</para>
        /// </summary>
        /// <param name="in_threadID">The thread to retrieve CPU information from.</param>
        public XeProcessorToken GetProcessorToken(int in_threadID = XeConsole.MainThreadID)
        {
            return new XeProcessorToken(_console, new XeThreadInfo(_console, in_threadID));
        }

        public void Dispose()
        {
            Detach();
        }
    }
}
