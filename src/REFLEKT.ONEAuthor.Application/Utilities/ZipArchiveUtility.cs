using System;
using System.IO;
using System.IO.Compression;

namespace REFLEKT.ONEAuthor.Application.Utilities
{
    public class ZipArchiveUtility : IDisposable
    {
        public string ExtractedDirectory { get; set; }

        public static ZipArchiveUtility ExtractToDirectory(string sourceArchiveName, string targetDirectory)
        {
            ZipFile.ExtractToDirectory(sourceArchiveName, targetDirectory, true);

            return new ZipArchiveUtility { ExtractedDirectory = targetDirectory };
        }

        public void Dispose()
        {
            Directory.Delete(ExtractedDirectory, true);
        }
    }
}