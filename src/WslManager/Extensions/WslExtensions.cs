using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WslManager.Models;

namespace WslManager.Extensions
{
    internal static class WslExtensions
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
                StandardOutputEncoding = Encoding.UTF8,
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
                .Replace("\0", string.Empty)
                .Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<string> ExecuteAndGetResultForWsl(this DistroInfoBase distro, string userName, string oneLinerBashScript)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return ExecuteAndGetResult("wsl.exe", $"--distribution {distro.DistroName} -- {oneLinerBashScript}");
            else
                return ExecuteAndGetResult("wsl.exe", $"--distribution {distro.DistroName} --user {userName} -- {oneLinerBashScript}");
        }

        public static IEnumerable<string> GetDistroNames()
        {
            return ExecuteAndGetResult("wsl.exe", "--list --quiet");
        }

        public static IEnumerable<DistroInfo> GetDistroList()
        {
            return ParseDistroList(ExecuteAndGetResult("wsl.exe", "--list --verbose"));
        }

        public static IEnumerable<DistroInfo> ParseDistroList(IEnumerable<string> lines)
        {
            var o = new List<DistroInfo>();

            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            var list = new List<DistroInfo>(Math.Max(0, lines.Count() - 1));

            foreach (var eachLine in lines)
            {
                var items = eachLine.Trim().Split(WhitespaceChars, StringSplitOptions.RemoveEmptyEntries);

                if (3 <= items.Length && items.Length <= 4)
                {
                    var info = new DistroInfo();

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

        public static Process CreateLaunchSpecificDistroProcess(this DistroInfoBase distroInfo)
        {
            var startInfo = new ProcessStartInfo("cmd.exe", $"/c wsl.exe --distribution {distroInfo.DistroName}")
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

        public static Process CreateLaunchSpecificDistroExplorerProcess(this DistroInfoBase distroInfo)
        {
            var startInfo = new ProcessStartInfo($"\\\\wsl$\\{distroInfo.DistroName}")
            {
                UseShellExecute = true,
            };

            var process = new Process()
            {
                StartInfo = startInfo,
            };

            return process;
        }

        public static Process CreateExportDistroProcess(this DistroInfoBase distroInfo, string tarFilePath)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--export {distroInfo.DistroName} \"{tarFilePath}\"")
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

        public static Process CreateImportDistroProcess(this DistroInfoBase distroInfo, string installDirectoryPath, string tarFilePath)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--import {distroInfo.DistroName} \"{installDirectoryPath}\" \"{tarFilePath}\"")
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

        public static Process CreateSetAsDefaultProcess(this DistroInfoBase distroInfo)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--set-default {distroInfo.DistroName}")
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

        public static Process CreateUnregisterDistroProcess(this DistroInfoBase distroInfo)
        {
            var startInfo = new ProcessStartInfo("wsl.exe", $"--unregister {distroInfo.DistroName}")
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

        public static IEnumerable<string> GetRegularUserList(this DistroInfoBase distroInfo)
        {
            return distroInfo.ExecuteAndGetResultForWsl("root", "cat /etc/passwd | grep \":[0-9][0-9][0-9][0-9]:\" | cut -d: -f1 -");
        }
    }
}
