using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WslManager
{
    internal static class WslHelper
    {
        public static string[] GetDistroNames()
        {
            var processStartInfo = new ProcessStartInfo("wsl.exe", "--list --quiet")
            {
                LoadUserProfile = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
                CreateNoWindow = true,
            };

            using var process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true,
            };

            if (!process.Start())
                throw new Exception("Cannot start the WSL process.");

            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd().Replace("\0", string.Empty);
            return output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static DistroInfoList GetDistroList()
        {
            var processStartInfo = new ProcessStartInfo("wsl.exe", "--list --verbose")
            {
                LoadUserProfile = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
                CreateNoWindow = true,
            };

            using var process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true,
            };

            if (!process.Start())
                throw new Exception("Cannot start the WSL process.");

            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd().Replace("\0", string.Empty);
            return new DistroInfoList(output);
        }

        public static Process CreateLaunchSpecificDistroProcess(string distroName)
        {
            var startInfo = new ProcessStartInfo("cmd.exe", $"/c wsl.exe --distribution {distroName}")
            {
                UseShellExecute = false,
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            };

            var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            return process;
        }

        public static Process CreateExportDistroProcess(string distroName, string tarFilePath)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--export \"{distroName}\" \"{tarFilePath}\"")
            {
                UseShellExecute = false,
            };

            var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            return process;
        }

        public static Process CreateImportDistroProcess(string distroName, string installDirectoryPath, string tarFilePath)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--import \"{distroName}\" \"{installDirectoryPath}\" \"{tarFilePath}\"")
            {
                UseShellExecute = false,
            };

            var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            return process;
        }

        public static Process CreateSetAsDefaultProcess(string distroName)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--set-default \"{distroName}\"")
            {
                UseShellExecute = false,
            };

            var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            return process;
        }

        public static Process CreateUnregisterDistroProcess(string distroName)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--unregister \"{distroName}\"")
            {
                UseShellExecute = false,
            };

            var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            return process;
        }
    }
}
