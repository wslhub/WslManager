using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace WslManager.External
{
    class LxRunOfflineInterface
    {
        private string lxRunOfflinePath;

        public LxRunOfflineInterface(string lxRunOfflinePath)
        {
            this.lxRunOfflinePath = lxRunOfflinePath;
        }

        private void ExecuteCommand(string command)
        {
            if (!File.Exists(lxRunOfflinePath))
                throw new FileNotFoundException("LxRunOffline.exe not found.");

            var proc = new ProcessStartInfo();

            proc.UseShellExecute = false;
            proc.FileName = lxRunOfflinePath;
            proc.Verb = "runas";
            proc.Arguments = command;
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.CreateNoWindow = true;

            var process = Process.Start(proc);
            process.WaitForExit();
            process.Close();
        }

        private string ExecuteCommandWithOutput(string command)
        {
            if (!File.Exists(lxRunOfflinePath))
                throw new FileNotFoundException("LxRunOffline.exe not found.");

            var proc = new ProcessStartInfo();

            proc.UseShellExecute = false;
            proc.FileName = lxRunOfflinePath;
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

        private string CoverAsString(string input)
        {
            return "\"" + input + "\"";
        }

        public string[] GetDistroList()
        {
            string res = ExecuteCommandWithOutput("list");
            if (res.Trim() == "")
                return null;

            return res.Remove(res.Length - 1).Split('\n').Select(p => p.Trim()).ToArray(); ;
        }

        public string GetDistroSummary(string distroName)
        {
            return ExecuteCommandWithOutput("summary -n " + distroName);
        }

        public string GetLxRunOfflineVersion()
        {
            return ExecuteCommandWithOutput("version");
        }

        public void MoveDistro(string distroName, string targetDir)
        {
            ExecuteCommand("move -n " + distroName + " -d " + CoverAsString(targetDir));
        }

        public void DuplicateDistro(string distroName, string targetDir, string newDistroName)
        {
            newDistroName = newDistroName.Replace(" ", "");
            ExecuteCommand("duplicate -n " + distroName + " -d " + CoverAsString(targetDir)
                + " -N " + newDistroName);
        }

        public void RegisterDistro(string newDistroName, string targetDir)
        {
            newDistroName = newDistroName.Replace(" ", "-");
            ExecuteCommand("register -n " + newDistroName + " -d " + CoverAsString(targetDir));
        }

        public void UnregisterDistro(string distroName)
        {
            ExecuteCommand("unregister -n " + distroName);
        }

        public Process RunDistro(string distroName, bool mount, bool runInBackground)
        {
            string command;
            if (mount)
                command = " run -n " + distroName;
            else
                command = " run -n " + distroName + " -w";

            if (!File.Exists(lxRunOfflinePath))
                throw new FileNotFoundException("LxRunOffline.exe not found.");

            var proc = new ProcessStartInfo();

            proc.UseShellExecute = false;
            proc.FileName = lxRunOfflinePath;
            proc.Verb = "runas";
            proc.Arguments = command;

            if (runInBackground)
            {
                proc.WindowStyle = ProcessWindowStyle.Hidden;
                proc.CreateNoWindow = true;
            }
            else
                proc.WindowStyle = ProcessWindowStyle.Normal;

            return Process.Start(proc);
        }

        public string GetDistroDir(string distroName)
        {
            return ExecuteCommandWithOutput("get-dir -n " + distroName);
        }

        public void CreateShortcut(string distroName, string targetPath)
        {
            ExecuteCommand("shortcut -n " + distroName + " -f " + targetPath);
        }

        public void CreateShortcut(string distroName, string targetPath, string targetIconPath)
        {
            ExecuteCommand("shortcut -n " + distroName + " -f " + targetPath + " -i " + targetIconPath);
        }

        public string GetDistroWslVersion(string distroName)
        {
            string[] rows = GetDistroSummary(distroName).Split('\n');
            foreach (string row in rows)
            {
                if (row.Contains("WSL version"))
                {
                    return int.Parse(row.Split(':')[1].Trim()).ToString();
                }
            }
            return "1";
        }

        public string GetDefaultDistro()
        {
            return ExecuteCommandWithOutput("get-default").Replace("\n", "").Trim();
        }

        public void OpenConsole()
        {
            if (!File.Exists(lxRunOfflinePath))
                throw new FileNotFoundException("LxRunOffline.exe not found.");

            var proc = new ProcessStartInfo();

            proc.FileName = "cmd.exe";
            proc.Verb = "runas";
            proc.Arguments = " /k " + CoverAsString(lxRunOfflinePath);
            proc.WindowStyle = ProcessWindowStyle.Normal;

            Process.Start(proc);
        }

    }
}
