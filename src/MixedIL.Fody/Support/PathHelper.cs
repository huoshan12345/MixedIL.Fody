namespace MixedIL.Fody.Support
{
    internal static class PathHelper
    {
        public static (string name, string ext) ExtractFileName(string fileName)
        {
            var index = fileName.LastIndexOf('.');
            return index >= 0
                ? (fileName.Substring(0, index), fileName.Substring(index))
                : (fileName, "");
        }
    }
}
