using System.IO;
using UnityEditor;
using UnityEngine;

namespace PackageToSource
{
    public class GitCloneRequest : IRequest
    {
        private Package _package = null;
        private string _path;

        public GitCloneRequest(Package package)
        {
            _package = package;
            _path = FileIO.Combine(Settings.gitProjectsPath, _package.displayName);

            if (FileIO.DirectoryExists(_path) && FileIO.IsEmptyDirectory(_path))
            {
                FileIO.DeleteDirectory(_path);
            }

            string gitCloneOutput = Git.Clone(Settings.gitProjectsPath, _package.url);
            Logger.Log(gitCloneOutput);

            string gitCheckoutOutput = Git.Checkout(_path, _package.hash);
            Logger.Log(gitCheckoutOutput);

            Logger.Log("CloneStarted");
        }

        public override bool Update()
        {
            if (FileIO.DirectoryExists(_path))
            {
                string gitPath = FileIO.Combine(_path, ".git");
                bool hasGit = FileIO.DirectoryExists(gitPath);

                string packagePath = FileIO.Combine(_path, "package.json");
                bool hasPackage = FileIO.FileExists(packagePath);

                if (hasGit && hasPackage)
                {
                    Logger.Log("Cloned");
                    return true;
                }
            }
            return false;
        }
    }
}