using System.Runtime.CompilerServices;

namespace XeSharp.Logger
{
    public static class XeLogger
    {
        private static List<IXeLogger> _handlers = [new XeConsoleLogger()];

        public static void Add(IXeLogger in_logger)
        {
            _handlers.Add(in_logger);
        }

        public static bool Remove(IXeLogger in_logger)
        {
            return _handlers.Remove(in_logger);
        }

        public static void Log(object in_message, EXeLogLevel in_logLevel, [CallerMemberName] string in_caller = null)
        {
            foreach (var logger in _handlers)
                logger.Log(in_message, in_logLevel, in_caller);
        }

        public static void Log(object in_message, [CallerMemberName] string in_caller = null)
        {
            Log(in_message, EXeLogLevel.None, in_caller);
        }

        public static void Log(object in_message)
        {
            Log(in_message, string.Empty);
        }

        public static void Utility(object in_message, [CallerMemberName] string in_caller = null)
        {
            Log(in_message, EXeLogLevel.Utility, in_caller);
        }

        public static void Utility(object in_message)
        {
            Utility(in_message, string.Empty);
        }

        public static void Warning(object in_message, [CallerMemberName] string in_caller = null)
        {
            Log(in_message, EXeLogLevel.Warning, in_caller);
        }

        public static void Warning(object in_message)
        {
            Warning(in_message, string.Empty);
        }

        public static void Error(object in_message, [CallerMemberName] string in_caller = null)
        {
            Log(in_message, EXeLogLevel.Error, in_caller);
        }

        public static void Error(object in_message)
        {
            Error(in_message, string.Empty);
        }

        public static void Write(object in_str, EXeLogLevel in_logLevel)
        {
            foreach (var logger in _handlers)
                logger.Write(in_str, in_logLevel);
        }

        public static void Write(object in_str)
        {
            Write(in_str, EXeLogLevel.None);
        }

        public static void WriteLine(object in_str, EXeLogLevel in_logLevel)
        {
            foreach (var logger in _handlers)
                logger.WriteLine(in_str, in_logLevel);
        }

        public static void WriteLine(object in_str)
        {
            WriteLine(in_str, EXeLogLevel.None);
        }
    }
}
