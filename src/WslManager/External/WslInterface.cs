using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WslManager.Models;

namespace WslManager.External
{
    class WslInterface
    {
        private string wslPath = "wsl";
        private WindowsVersionManager windowsVersionManager;

        public WslInterface(WindowsVersionManager windowsVersionManager)
        {
            this.windowsVersionManager = windowsVersionManager;
        }

        private void ExecuteCommand(string command, bool wsl)
        {
            var proc = new ProcessStartInfo();

            proc.UseShellExecute = false;
            proc.FileName = wsl ? wslPath : "cmd.exe";
            proc.Verb = "runas";
            proc.Arguments = command;
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.CreateNoWindow = true;

            var process = Process.Start(proc);
            process.WaitForExit();
            process.Close();
        }

        private String ExecuteCommandWithOutput(string command)
        {
            var proc = new ProcessStartInfo();

            proc.UseShellExecute = false;
            proc.FileName = wslPath;
            proc.Verb = "runas";
            proc.Arguments = command;
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StandardOutputEncoding = Encoding.Unicode;
            proc.RedirectStandardOutput = true;
            proc.CreateNoWindow = true;

            var process = Process.Start(proc);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();

            return output;
        }

        public void TerminateDistro(string distroName)
        {
            if (windowsVersionManager.CurrentVersion >= WindowsVersions.V1903)
            {
                ExecuteCommand("--terminate " + distroName, true);
            }
            else if (windowsVersionManager.CurrentVersion == WindowsVersions.V1809)
            {
                ExecuteCommand("/c wslconfig /terminate " + distroName, false);
            }
            else
            {
                TerminateAllDistros();
            }
        }

        public void TerminateAllDistros()
        {
            if (windowsVersionManager.CurrentVersion >= WindowsVersions.V2004)
            {
                ExecuteCommand("--shutdown", true);
            }
            else
            {
                ExecuteCommand("/c net stop LxssManager", false);
            }
        }

        public void SetVersion(string distroName, int targetVersion)
        {
            if (windowsVersionManager.CurrentVersion >= WindowsVersions.V2004)
            {
                ExecuteCommand("--set-version " + distroName + " " + targetVersion.ToString(), true);
            }
        }

        public void SetDefaultVersion(int targetVersion)
        {
            if (windowsVersionManager.CurrentVersion >= WindowsVersions.V2004)
            {
                ExecuteCommand("--set-default-version " + targetVersion.ToString(), true);
            }
        }

        public string[] GetRunningDistros()
        {
            if (windowsVersionManager.CurrentVersion >= WindowsVersions.V1903)
            {
                List<String> output = ExecuteCommandWithOutput("--list --running").Split('\n').Select(p => p.Trim()).ToList();
                output.RemoveAt(0);
                output.RemoveAt(output.Count - 1);
                for (int i = 0; i < output.Count; i++)
                {
                    if (output[i].Contains(" "))
                        output[i] = output[i].Split(' ')[0];
                }
                return output.ToArray();
            }
            return null;
        }

        public void OpenConsole()
        {
            var proc = new ProcessStartInfo();

            proc.FileName = "cmd.exe";
            proc.Verb = "runas";
            proc.Arguments = " /k wsl --help";
            proc.WindowStyle = ProcessWindowStyle.Normal;

            Process.Start(proc);
        }

    }
}
