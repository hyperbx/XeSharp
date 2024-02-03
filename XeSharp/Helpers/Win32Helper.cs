namespace XeSharp.Helpers
{
    public class Win32Helper
    {
        /// <summary>
        /// Determines whether the input HRESULT is successful.
        /// </summary>
        /// <param name="in_hResult">The HRESULT to check.</param>
        public static bool IsHResultSuccess(int in_hResult)
        {
            return in_hResult >= 0;
        }

        /// <summary>
        /// Determines whether the input HRESULT has failed.
        /// </summary>
        /// <param name="in_hResult">The HRESULT to check.</param>
        public static bool IsHResultFail(int in_hResult)
        {
            return in_hResult < 0;
        }

        /// <summary>
        /// Gets the path to the user's directory.
        /// </summary>
        public static string GetUserDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
    }
}
