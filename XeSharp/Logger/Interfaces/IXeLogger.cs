using System.Runtime.CompilerServices;

namespace XeSharp.Logger
{
    public interface IXeLogger
    {
        public void Log(object in_message, EXeLogLevel in_logLevel, [CallerMemberName] string in_caller = null);
        public void Write(object in_str, EXeLogLevel in_logLevel);
        public void WriteLine(object in_str, EXeLogLevel in_logLevel);
    }
}
