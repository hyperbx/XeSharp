namespace XeSharp.Logger
{
    public class XeConsoleLogger : IXeLogger
    {
        public void Log(object in_message, EXeLogLevel in_logLevel, string in_caller)
        {
            WriteLine(string.IsNullOrEmpty(in_caller) ? in_message : $"[{in_caller}] {in_message}", in_logLevel);
        }

        public void Write(object in_str, EXeLogLevel in_logLevel)
        {
            var oldColour = Console.ForegroundColor;

            switch (in_logLevel)
            {
                case EXeLogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case EXeLogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case EXeLogLevel.Utility:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }

            Console.Write(in_str);

            Console.ForegroundColor = oldColour;
        }

        public void WriteLine(object in_str, EXeLogLevel in_logLevel)
        {
            Write(in_str.ToString().Replace("\n", "\r\n") + "\r\n", in_logLevel);
        }
    }
}
