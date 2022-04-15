using System;

namespace PackageToSource
{
    public static class Settings
    {
        public static string shellName;
        public static bool debugLogger;


        static Settings()
        {
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
        }
    }
}


