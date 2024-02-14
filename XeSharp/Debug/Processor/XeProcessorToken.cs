using XeSharp.Device;
using XeSharp.Device.Title;

namespace XeSharp.Debug.Processor
{
    /// <summary>
    /// Creates a snapshot of the current processor state on the remote console.
    /// <para>This type will commit all changes to the remote console upon disposing it manually or via a "using" clause.</para>
    /// </summary>
    /// <param name="in_console">The console to get CPU information from.</param>
    /// <param name="in_thread">The thread to get CPU information from.</param>
    public class XeProcessorToken(XeConsole in_console, XeThreadInfo in_thread) : XeProcessor(in_console, in_thread, false), IDisposable
    {
        /// <summary>
        /// Creates a snapshot of the current processor state on the remote console.
        /// <para>This type will commit all changes to the remote console upon disposing it manually or via a "using" clause.</para>
        /// </summary>
        /// <param name="in_console">The console to get CPU information from.</param>
        /// <param name="in_threadID">The thread ID to get CPU information from.</param>
        public XeProcessorToken(XeConsole in_console, int in_threadID)
            : this(in_console, new XeThreadInfo(in_console, in_threadID)) { }

        public void Dispose()
        {
            CommitAll();
        }
    }
}
