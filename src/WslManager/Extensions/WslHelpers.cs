using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WslManager.Models;
using WslManager.ViewModels;

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

        public static IEnumerable<string> ExecuteAndGetResultForLinux(string executablePath, string commandLineArguments)
        {
            var processStartInfo = new ProcessStartInfo(executablePath, commandLineArguments)
            {
                LoadUserProfile = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = new UTF8Encoding(false),
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

        public static IEnumerable<string> ExecuteAndGetResultForWsl(string distroName, string userName, string oneLinerBashScript)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return ExecuteAndGetResultForLinux("wsl.exe", $"--distribution {distroName} -- {oneLinerBashScript}");
            else
                return ExecuteAndGetResultForLinux("wsl.exe", $"--distribution {distroName} --user {userName} -- {oneLinerBashScript}");
        }

        public static IEnumerable<string> GetDistroNames()
        {
            return ExecuteAndGetResult("wsl.exe", "--list --quiet");
        }

        public static IEnumerable<WslDistro> GetDistroList()
        {
            return ParseDistroList(ExecuteAndGetResult("wsl.exe", "--list --verbose"));
        }

        public static IEnumerable<LinuxUserInfo> GetLinuxUserInfo(string distroName, string userName = "root")
        {
            var results = new List<LinuxUserInfo>();

            try
            {
                var passwdContents = ExecuteAndGetResultForWsl(distroName, userName, "cat /etc/passwd");

                foreach (var eachLine in passwdContents)
                    results.Add(new LinuxUserInfo(eachLine));
            }
            catch { }

            return results.AsReadOnly();
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

        public static Process CreateLaunchSpecificDistroAsUserProcess(string distroName, string userName, string execCommandLine)
        {
            var startInfo = new ProcessStartInfo("cmd.exe", $"/c wsl.exe --distribution {distroName} --user {userName} -- {execCommandLine}")
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

        public static string GetArchitectureName()
        {
            var value = RuntimeInformation.OSArchitecture;

            switch (value)
            {
                case Architecture.X64: return "amd64";
                default: return value.ToString().ToLowerInvariant();
            }
        }

        public static Guid? GetDistroGuid(string distroName)
        {
            using var lxssKey = Registry.CurrentUser.OpenSubKey(
                Path.Combine("SOFTWARE", "Microsoft", "Windows", "CurrentVersion", "Lxss"),
                false);

            foreach (var eachSubKey in lxssKey.GetSubKeyNames())
            {
                using var subKey = lxssKey.OpenSubKey(eachSubKey, false);
                var subDistroName = subKey.GetValue("DistributionName", default, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;

                if (!string.Equals(subDistroName, distroName, StringComparison.Ordinal))
                    continue;

                var keyName = Path.GetFileName(subKey.Name);
                if (!Guid.TryParse(keyName, out Guid value))
                    continue;

                return value;
            }

            return default;
        }

        public static string GetDistroLocation(Guid distroGuid)
        {
            using var distroKey = Registry.CurrentUser.OpenSubKey(
                Path.Combine("SOFTWARE", "Microsoft", "Windows", "CurrentVersion", "Lxss", distroGuid.ToString("B")),
                false);

            if (distroKey == null)
                return null;

            var basePath = distroKey.GetValue("BasePath", default) as string;

            if (string.IsNullOrEmpty(basePath))
                return default;

            basePath = Path.GetFullPath(basePath);

            if (!Directory.Exists(basePath))
                return null;

            return basePath;
        }
    }
}
