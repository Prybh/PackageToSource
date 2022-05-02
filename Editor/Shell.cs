using System;
using System.Diagnostics;

namespace PackageToSource
{
    public static class Shell
    {
        public static string ExecuteCommand(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                FileName = Settings.shellName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = command
            };

            return ExecuteProcess(processInfo);
        }

        public static string ExecuteProcess(ProcessStartInfo processInfo)
        {
            if (processInfo.FileName == null || processInfo.FileName.Length == 0)
                return "";

            string output = "";

            Process process = Process.Start(processInfo);
            try
            {
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                output = e.ToString();
            }
            finally
            {
                process.Close();
            }

            return output;
        }
    }
}
