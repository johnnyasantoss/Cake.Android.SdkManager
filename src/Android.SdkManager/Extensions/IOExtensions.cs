using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Android.SdkManager.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IOExtensions
    {
        public static DirectoryInfo ToDirectoryInfo(this string dirPath)
        {
            return new DirectoryInfo(dirPath);
        }

        public static FileSystemInfo Combine(this DirectoryInfo dir, string relativePath)
        {
            var fullPath = Path.Combine(dir.FullName, relativePath);

            if (relativePath.EndsWith("/") || relativePath.EndsWith("\\"))
                return new DirectoryInfo(fullPath);

            return new FileInfo(fullPath);
        }

        public static void EmptyDir(this DirectoryInfo dir)
        {
            if (dir.Exists)
            {
                dir.Delete(true);
                dir.Create();
            }
            else
            {
                dir.Create();
            }
        }

        public static FileInfo ToFileInfo(this string filePath)
        {
            return new FileInfo(filePath);
        }

        public static string GetSha1Checksum(this FileInfo fileInfo)
        {
            using var fileReader = fileInfo.OpenRead();
            using var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(fileReader);
            return hashBytes.ToHexString();
        }

        public static bool ValidateFileSha1(this FileInfo fileInfo, string expectedSha1Checksum)
        {
            var sha1Checksum = fileInfo.GetSha1Checksum();
            return expectedSha1Checksum.Equals(sha1Checksum, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EnsureOnlyValidFileExists(this FileInfo tempFile, string expectedSha1Checksum)
        {
            if (!tempFile.Exists)
                return false;

            var hashCachedFile = tempFile.GetSha1Checksum();
            if (hashCachedFile.Equals(expectedSha1Checksum, StringComparison.Ordinal))
                return true;

            tempFile.Delete();

            return false;
        }

        public static void ExtractOverDirectory(this ZipArchive zipArchive, DirectoryInfo destDir)
        {
            foreach (var zipEntry in zipArchive.Entries)
            {
                var fsInfo = destDir.Combine(zipEntry.FullName);

                switch (fsInfo)
                {
                    case DirectoryInfo dir:
                        if (!dir.Exists)
                            dir.Create();
                        break;
                    case FileInfo file:
                        zipEntry.ExtractToFile(file.FullName, true);
                        break;
                }
            }
        }
    }
}
