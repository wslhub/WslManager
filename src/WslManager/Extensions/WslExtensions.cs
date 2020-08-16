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

        public static IEnumerable<DistroInfo> GetDistroList()
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
            return ParseDistroList(output);
        }

        public static IEnumerable<DistroInfo> ParseDistroList(string expression)
        {
            var o = new List<DistroInfo>();

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var lines = expression.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<DistroInfo>(Math.Max(0, lines.Length - 1));

            foreach (var eachLine in lines)
            {
                var items = eachLine.Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

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
    }
}
