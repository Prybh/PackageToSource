using System;

namespace PackageToSource
{
    public static class Git
    {
        public static bool IsPresent()
        {
            return Shell.ExecuteCommand("git --version").StartsWith("git version");
        }

        public static string Clone(string path, string address)
        {
            return Shell.ExecuteCommand("cd " +  path + " | git clone " + address);
        }

        public static string Checkout(string repoPath, string id)
        {
            return Shell.ExecuteCommand("cd " + repoPath + " | git checkout " + id);
        }

        public static string GetBranchName(string repoPath)
        {
            return Shell.ExecuteCommand("cd " + repoPath + " | git rev-parse --abbrev-ref HEAD").Trim();
        }

        public static string GetTagName(string repoPath)
        {
            return Shell.ExecuteCommand("cd " + repoPath + " | git describe --tags").Trim();
        }

        public static int GetFilesChanged(string repoPath)
        {
            string output = Shell.ExecuteCommand("cd " + repoPath + " | git diff --stat | tail -n1");

            int firstSpacePos = output.IndexOf(' ');
            if (output.Length > 0 && firstSpacePos > 0)
            {
                int filesChanged;
                if (int.TryParse(output.Substring(0, firstSpacePos), out filesChanged))
                {
                    return filesChanged;
                }
            }

            return 0;
        }
    }
}
