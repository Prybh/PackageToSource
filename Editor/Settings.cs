using System;

namespace PackageToSource
{
    public static class Settings
    {
        public static string gitProjectsPath;
        public static string shellName;
        public static bool debugLogger;
        public static bool deleteOnUnused;


        static Settings()
        {
            gitProjectsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            shellName = GetDefaultShellName();

            debugLogger = false;

            deleteOnUnused = false;
        }

        public static string GetDefaultShellName()
        {
#if UNITY_EDITOR_WIN
            return "powershell.exe";
#elif UNITY_EDITOR_OSX
            return "/bin/bash";
#elif UNITY_EDITOR_LINUX
            return "/bin/bash";
#else
            return "";
#endif
        }
    }
}


