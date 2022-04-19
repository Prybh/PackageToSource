using System;
using System.IO;
using System.Linq;

namespace PackageToSource
{
    public static class FileIO
    {
        public static string Combine(string a, string b)
        {
            string p = Path.Combine(a, b);
            p.Replace('\\', '/');
            return p;
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public static bool IsEmptyDirectory(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public static void DeleteDirectory(string path)
        {
            if (!DirectoryExists(path))
                return;

            File.SetAttributes(path, FileAttributes.Normal);

            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(path, false);
        }
    }
}
