using System.Runtime.CompilerServices;

namespace XeSharp.Logger
{
    public interface IXeLogger
    {
        public void Log(object in_message, EXeLogLevel in_logLevel, [CallerMemberName] string in_caller = null);
    }
}
