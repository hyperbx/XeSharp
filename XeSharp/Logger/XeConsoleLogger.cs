namespace XeSharp.Logger
{
    public class XeConsoleLogger : IXeLogger
    {
        public void Log(object in_message, EXeLogLevel in_logLevel, string in_caller)
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

            var message = in_message.ToString().Replace("\n", "\r\n");

            Console.WriteLine(string.IsNullOrEmpty(in_caller) ? message : $"[{in_caller}] {message}");

            Console.ForegroundColor = oldColour;
        }
    }
}
