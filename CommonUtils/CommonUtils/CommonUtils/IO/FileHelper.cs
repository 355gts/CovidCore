namespace CommonUtils.IO
{
    public class FileHelper : IFileHelper
    {
        public bool FileExists(string filePath)
        {
            return IOHelper.FileExists(filePath);
        }
    }
}
