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
            return Shell.ExecuteCommand("cd " + repoPath + " | git checkout " + id).Trim();
        }

        public static string GetBranchName(string repoPath)
        {
            return Shell.ExecuteCommand("cd " + repoPath + " | git rev-parse --abbrev-ref HEAD").Trim();
        }

        public static string GetTagName(string repoPath)
        {
            return Shell.ExecuteCommand("cd " + repoPath + " | git describe --tags").Trim();
        }

        public static string GetCommitSha(string repoPath)
        {
            return Shell.ExecuteCommand("cd " + repoPath + " | git rev-parse HEAD").Trim();
        }

        public static string GetRemoteUrl(string repoPath)
        {
            return Shell.ExecuteCommand("cd " + repoPath + " | git config --get remote.origin.url").Trim();
        }

        public static int GetFilesChanged(string repoPath)
        {
            string output = Shell.ExecuteCommand("cd " + repoPath + " | git diff --shortstat").Trim();

            int firstSpacePos = output.IndexOf(' ');
            if (output.Length > 0 && firstSpacePos >= 0)
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
