namespace XeSharp.Helpers
{
    public class FileSystemHelper
    {
        public static long GetDirectorySize(string in_path, bool in_isThrowOnError = true)
        {
            var totalSize = 0L;

            try
            {
                foreach (string file in Directory.GetFiles(in_path, "*", SearchOption.AllDirectories))
                    totalSize += new FileInfo(file).Length;
            }
            catch
            {
                if (in_isThrowOnError)
                    throw;

                return totalSize;
            }

            return totalSize;
        }
    }
}
