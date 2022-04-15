using System;

namespace PackageToSource
{
    public enum Log { Info, Warning, Error }

    public static class Logger
    {
        public static void Log(string message, Log logLevel = PackageToSource.Log.Info, bool debugOnly = false)
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
                    if (!Settings.debugLogger || (debugOnly && Settings.debugLogger))
                    {
                        UnityEngine.Debug.LogWarning(message);
                    }
                    break;
                case PackageToSource.Log.Error:
                    if (!Settings.debugLogger || (debugOnly && Settings.debugLogger))
                    {
                        UnityEngine.Debug.LogError(message);
                    }
                    break;
            }
        }
    }
}

