using System.IO;

namespace CommonUtils.IO
{
    public static class IOHelper
    {
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
