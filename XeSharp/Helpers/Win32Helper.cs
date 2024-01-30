namespace XeSharp.Helpers
{
    public class Win32Helper
    {
        public static bool IsHResultSuccess(int in_hResult)
        {
            return in_hResult >= 0;
        }

        public static bool IsHResultFail(int in_hResult)
        {
            return in_hResult < 0;
        }
    }
}
