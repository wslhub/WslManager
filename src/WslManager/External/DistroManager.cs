using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WslManager.Models;

namespace WslManager.External
{
    static class DistroManager
    {
        public static WindowsVersionManager windowsVersionManager = new WindowsVersionManager();
        public static LxRunOfflineInterface lxRunOfflineInterface = new LxRunOfflineInterface("External\\LxRunOffline.exe");
        public static WslInterface wslInterface = new WslInterface(windowsVersionManager);

        public static List<WslDistro> wslDistroList = new List<WslDistro>();

        public static void RefreshWslDistroData()
        {
            string[] distroNames = lxRunOfflineInterface.GetDistroList();

            if (distroNames == null)
                return;

            string[] runningDistros = wslInterface.GetRunningDistros();

            foreach (WslDistro distroItem in wslDistroList)
            {
                if (!distroNames.Any(distroItem.DistroName.Equals))
                    wslDistroList.Remove(distroItem);
            }

            for (int i = 0; i < distroNames.Length; i++)
            {
                if (wslDistroList.Any(d => d.DistroName == distroNames[i]))
                {
                    WslDistro wslDistro = wslDistroList.Find(d => d.DistroName == distroNames[i]);
                    wslDistro.WSLVersion = lxRunOfflineInterface.GetDistroWslVersion(distroNames[i]);

                    if (windowsVersionManager.CurrentVersion >= WindowsVersions.V1903)
                    {
                        if (runningDistros.Any(distroNames[i].Equals))
                            wslDistro.DistroStatus = "Running";
                        else
                            wslDistro.DistroStatus = "Stopped";
                    }
                    else
                    {
                        wslDistro.DistroStatus = "Unknown";
                    }
                }
                else
                {
                    WslDistro wslDistro = new WslDistro();

                    wslDistro.DistroName = distroNames[i];
                    wslDistro.WSLVersion = lxRunOfflineInterface.GetDistroWslVersion(distroNames[i]);

                    if (windowsVersionManager.CurrentVersion >= WindowsVersions.V1903)
                    {
                        if (runningDistros.Any(distroNames[i].Equals))
                            wslDistro.DistroStatus = "Running";
                        else
                            wslDistro.DistroStatus = "Stopped";
                    }
                    else
                    {
                        wslDistro.DistroStatus = "Unknown";
                    }

                    wslDistroList.Add(wslDistro);
                }
            }
        }

        public static void StartDistro(string distroName)
        {
            Process p = lxRunOfflineInterface.RunDistro(distroName, false, false);

            RefreshWslDistroData();
        }

        public static void OpenDistroFolder(string distroName)
        {
            var startInfo = new ProcessStartInfo(
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
                lxRunOfflineInterface.GetDistroDir(distroName))
            {
                UseShellExecute = false,
            };

            Process.Start(startInfo);
        }

        public static void ExploreDistro(string distroName)
        {
            if (windowsVersionManager.CurrentVersion >= WindowsVersions.V1903)
            {
                lxRunOfflineInterface.RunDistro(distroName, false, true);

                var startInfo = new ProcessStartInfo(
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
                $@"\\wsl$\{distroName}")
                {
                    UseShellExecute = false,
                };

                Process.Start(startInfo);

                RefreshWslDistroData();
            }
            else
            {
                OpenDistroFolder(distroName);
            }
        }

        public static void MountDistro(string distroName)
        {
            lxRunOfflineInterface.RunDistro(distroName, true, false);

            RefreshWslDistroData();
        }

        public static void RenameDistro(string distroName, string newDistroName)
        {
            string path = lxRunOfflineInterface.GetDistroDir(distroName);

            wslInterface.TerminateAllDistros();

            lxRunOfflineInterface.UnregisterDistro(distroName);
            lxRunOfflineInterface.RegisterDistro(newDistroName, path);

            RefreshWslDistroData();
        }

        public static void MoveDistro(string distroName, string targetPath)
        {
            wslInterface.TerminateAllDistros();

            lxRunOfflineInterface.MoveDistro(distroName, targetPath);

            RefreshWslDistroData();
        }

        public static void DuplicateDistor(string distroName, string newDistroName, string path)
        {
            wslInterface.TerminateAllDistros();

            lxRunOfflineInterface.DuplicateDistro(distroName, path, newDistroName);

            RefreshWslDistroData();
        }

        public static void ExportDistro()
        {
            // todo
        }

        public static void SwitchDistroVersion(string distroName, int targetVer)
        {
            wslInterface.TerminateDistro(distroName);
            wslInterface.SetVersion(distroName, targetVer);

            RefreshWslDistroData();
        }

        public static void UnregisterDistro(string distroName)
        {
            wslInterface.TerminateDistro(distroName);
            lxRunOfflineInterface.UnregisterDistro(distroName);
            RefreshWslDistroData();
        }

        public static void TerminateDistro(string distroName)
        {
            wslInterface.TerminateDistro(distroName);
            RefreshWslDistroData();
        }

        public static void DeleteDistro(string distroName)
        {
            wslInterface.TerminateAllDistros();
            string path = lxRunOfflineInterface.GetDistroDir(distroName);

            System.IO.DirectoryInfo directory = new DirectoryInfo(path);

            lxRunOfflineInterface.UnregisterDistro(distroName);

            directory.Delete(true);
        }

        public static void ImportDistro()
        {
            // todo
        }

        public static void ShutdownWsl()
        {
            wslInterface.TerminateAllDistros();
            RefreshWslDistroData();
        }

        public static void RegisterDistro(string newDistroName, string path)
        {
            wslInterface.TerminateAllDistros();
            lxRunOfflineInterface.RegisterDistro(newDistroName, path);
            RefreshWslDistroData();
        }

        public static void OpenLxRunOfflineConsole()
        {
            lxRunOfflineInterface.OpenConsole();
        }

        public static void OpenWslConsole()
        {
            wslInterface.OpenConsole();
        }

        public static void SetCustomWindowsVersion(int targetVersion)
        {
            windowsVersionManager.CurrentVersion = targetVersion;
        }
    }
}
