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

#if UNITY_EDITOR_WIN
            shellName = "powershell.exe";
#endif
#if UNITY_EDITOR_OSX
            shellName = "/bin/bash";
#endif
#if UNITY_EDITOR_LINUX
            shellName = "/bin/bash";
#endif

            debugLogger = true;

            deleteOnUnused = false;
        }
    }
}


