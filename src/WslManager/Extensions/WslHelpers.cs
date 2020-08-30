using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WslManager.Models;

namespace WslManager.Extensions
{
    internal static class WslHelpers
    {
        private static readonly char[] NewLineChars = new char[] { '\r', '\n', };

        private static readonly char[] WhitespaceChars = new char[] { '\u0020', '\t', };

        public static IEnumerable<string> ExecuteAndGetResult(string executablePath, string commandLineArguments)
        {
            var processStartInfo = new ProcessStartInfo(executablePath, commandLineArguments)
            {
                LoadUserProfile = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.Unicode,
                CreateNoWindow = true,
            };

            using var process = new Process()
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true,
            };

            if (!process.Start())
                throw new Exception("Cannot start the WSL process.");

            return process.StandardOutput
                .ReadToEnd()
                .Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<string> ExecuteAndGetResultForWsl(string distroName, string userName, string oneLinerBashScript)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return ExecuteAndGetResult("wsl.exe", $"--distribution {distroName} -- {oneLinerBashScript}");
            else
                return ExecuteAndGetResult("wsl.exe", $"--distribution {distroName} --user {userName} -- {oneLinerBashScript}");
        }

        public static IEnumerable<string> GetDistroNames()
        {
            return ExecuteAndGetResult("wsl.exe", "--list --quiet");
        }

        public static IEnumerable<WslDistro> GetDistroList()
        {
            return ParseDistroList(ExecuteAndGetResult("wsl.exe", "--list --verbose"));
        }

        public static IEnumerable<WslDistro> ParseDistroList(IEnumerable<string> lines)
        {
            var o = new List<WslDistro>();

            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            var list = new List<WslDistro>(Math.Max(0, lines.Count() - 1));

            foreach (var eachLine in lines)
            {
                var items = eachLine.Trim().Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

                if (3 <= items.Length && items.Length <= 4)
                {
                    var info = new WslDistro();

                    if (items[0] == "*")
                    {
                        items = items.Skip(1).ToArray();
                        info.IsDefault = true;
                    }

                    info.DistroName = items[0];
                    info.DistroStatus = items[1];
                    info.WSLVersion = items[2];

                    if (!string.Equals(info.DistroName, "NAME", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(info.DistroStatus, "STATE", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(info.WSLVersion, "VERSION", StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(info);
                    }
                }
            }

            return list.AsReadOnly();
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

        public static Process CreateTerminateSpecificDistroProcess(string distroName)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--terminate {distroName}")
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

        public static Process CreateLaunchSpecificDistroExplorerProcess(string distroName)
        {
            var startInfo = new ProcessStartInfo($"\\\\wsl$\\{distroName}")
            {
                UseShellExecute = true,
            };

            var process = new Process()
            {
                StartInfo = startInfo,
            };

            return process;
        }

        public static Process CreateExportDistroProcess(string distroName, string tarFilePath)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--export {distroName} \"{tarFilePath}\"")
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

        public static Process CreateImportDistroProcess(string newName, string installDirectoryPath, string tarFilePath)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--import {newName} \"{installDirectoryPath}\" \"{tarFilePath}\"")
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
            var startInfo = new ProcessStartInfo("wsl.exe", $"--set-default {distroName}")
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
            var startInfo = new ProcessStartInfo("wsl.exe", $"--unregister {distroName}")
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

        public static Process CreateShutdownDistroProcess()
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--shutdown")
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

        public static IEnumerable<string> GetRegularUserList(string distroName)
        {
            return ExecuteAndGetResultForWsl(distroName, "root", "cat /etc/passwd | grep \":[0-9][0-9][0-9][0-9]:\" | cut -d: -f1 -");
        }
    }
}
