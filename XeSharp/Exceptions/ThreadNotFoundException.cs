namespace XeSharp.Exceptions
{
    public class ThreadNotFoundException(int in_threadID) : Exception(string.Format(_message, in_threadID))
    {
        private const string _message = "No such thread exists: {0}";
    }
}
