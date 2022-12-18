using Newtonsoft.Json;

namespace WarOfRightsUnpack.Utility
{
    public static class FileHelper
    {
        public static FileInfo GetFile(DirectoryInfo directoryInfo, string fileName)
        {
            var results = directoryInfo.GetFiles(fileName, SearchOption.AllDirectories).ToArray();
            if (results.Length > 1)
            {
                Console.WriteLine($"Found more than one '\"{fileName}\"' file within the specified directory '{directoryInfo.FullName}'.");
                Console.WriteLine("Taking the first occurrence an proceeding...");
            }
            else if (results.Length == 0)
            {
                throw new ArgumentNullException(
                    message: $"Cannot find '\"{fileName}\"' file within the specified directory '{directoryInfo.FullName}'.",
                    paramName: nameof(results));
            }

            return results.First();
        }
    }
}
