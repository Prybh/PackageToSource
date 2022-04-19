using System;

namespace PackageToSource
{
    public enum Log { Info, Warning, Error }

    public static class Logger
    {
        public static void Log(string message, Log logLevel = PackageToSource.Log.Info)
        {
            switch (logLevel)
            {
                case PackageToSource.Log.Info:
                    if (Settings.debugLogger)
                    {
                        UnityEngine.Debug.Log(message);
                    }
                    break;
                case PackageToSource.Log.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                case PackageToSource.Log.Error:
                    UnityEngine.Debug.LogError(message);
                    break;
            }
        }
    }
}

